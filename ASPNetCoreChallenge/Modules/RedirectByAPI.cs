using System.Diagnostics;
using ASPNetCoreChallenge.Models;
using Microsoft.Extensions.Caching.Memory;
using RestSharp;

namespace ASPNetCoreChallenge.Modules
{
    public interface IRedirectByAPI
    {
        void Attach(WebApplication app);
    }

    public class RedirectByAPI : IRedirectByAPI
    {
        private string baseUrl = "";
        private string apiPath = "";
        private double cacheTimeout = 2.0;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        public RedirectByAPI(IMemoryCache cache, ILogger<RedirectByAPI> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public void Attach(WebApplication app)
        {
            app.Use(RoutByAPI);
            LoadOptions();
            StartCachingRefreshService();
        }

        async Task RoutByAPI(HttpContext context, RequestDelegate next)
        {
            bool redirect = false;
            var uriBuilder = GetUri(context.Request);

            if (_cache.TryGetValue("_apiRedirectCache", out IEnumerable<RedirectRule> redirectRules))
            {
                if (redirectRules == null || redirectRules.Count() < 0) return;
                foreach (var rule in redirectRules)
                {
                    if (rule.useRelative)
                    {
                        if (uriBuilder.Path.Contains(rule.redirectUrl, StringComparison.InvariantCultureIgnoreCase))
                        {
                            uriBuilder.Path = uriBuilder.Path.Replace(rule.redirectUrl, rule.targetUrl, StringComparison.InvariantCultureIgnoreCase);
                            context.Response.Redirect(uriBuilder.Uri.AbsoluteUri, rule.redirectType == 301);
                            redirect = true;
                        }
                    }
                    else
                    {
                        if (uriBuilder.Path.Equals(rule.redirectUrl, StringComparison.InvariantCultureIgnoreCase))
                        {
                            uriBuilder.Path = rule.targetUrl;
                            context.Response.Redirect(uriBuilder.Uri.AbsolutePath, rule.permanent);
                            redirect = true;
                        }
                    }
                }
            }
            if (!redirect)
                await next(context);
        }

        void StartCachingRefreshService()
        {
            RefreshCache();
            var timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = cacheTimeout * 60000;
            timer.AutoReset = true;
            timer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            RefreshCache();
        }

        void RefreshCache()
        {
            try
            {
                var options = new RestClientOptions(baseUrl)
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };
                RestClient client = new RestClient(options);
                var redirects = client.Get<RedirectRule[]>(new RestRequest(apiPath));
                _cache.Set("_apiRedirectCache", redirects);
                _logger.LogInformation("Redirect cache refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(101, ex, $"Unable to reach api at {baseUrl + apiPath}");
            }
        }

        void LoadOptions()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            baseUrl = config.GetValue<string>("AppSettings:RedirectByAPI.baseUrl");
            apiPath = config.GetValue<string>("AppSettings:RedirectByAPI.apiPath");
            cacheTimeout = config.GetValue<double>("AppSettings:RedirectByAPI.cacheTimeout");
        }

        UriBuilder GetUri(HttpRequest request)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
                Port = request.Host.Port.GetValueOrDefault(80),
                Path = request.Path.ToString(),
                Query = request.QueryString.ToString()
            };
            return uriBuilder;
        }
    }

    public static class RedirectByAPIExtension
    {
        public static void UseAPIRouting(this WebApplication app)
        {
            IRedirectByAPI? redirectByAPI = app.Services.GetService<IRedirectByAPI>();
            if (redirectByAPI != null)
            {
                redirectByAPI.Attach(app);
            }
        }
    }
}
