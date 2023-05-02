using System.Diagnostics;
using ASPNetCoreChallenge.Interfaces;
using ASPNetCoreChallenge.Models;
using Microsoft.Extensions.Caching.Memory;
using RestSharp;

namespace ASPNetCoreChallenge.Modules
{

    /// <summary>
    /// Redirects users using rules retrieved from API Endpoint
    /// </summary>
    public class RedirectByAPI : IRedirectByAPI
    {
        //Appsettings
        private string baseUrl = "";
        private string apiPath = "";
        private double cacheTimeout = 2.0;

        //Services
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        public RedirectByAPI(IMemoryCache cache = null, ILogger<RedirectByAPI> logger = null)
        {
            _cache = cache ?? new MemoryCache(null);
            _logger = logger;
        }

        /// <summary>
        /// Attaches the APIRedirect module to the current application
        /// </summary>
        /// <param name="app"></param>
        public void Attach(IApplicationBuilder app)
        {
            app.Use(RoutByAPI);
            LoadOptions();
            StartCachingRefreshService();
        }

        /// Called each time a page is hit, before routing occurs
        async Task RoutByAPI(HttpContext context, RequestDelegate next)
        {
            // holds whether or not we are redirecting
            bool redirect = false;
            var uriBuilder = GetUri(context.Request);
            // Check to see if redirects are in cache. If not, do nothing
            if (_cache.TryGetValue("_apiRedirectCache", out IEnumerable<RedirectRule> redirectRules))
            {
                // If api returns no rules, variable can be null
                if (redirectRules == null || redirectRules.Count() < 0) return;
                foreach (var rule in redirectRules)
                {
                    if (rule.useRelative)
                    {
                        if (uriBuilder.Path.Contains(rule.redirectUrl, StringComparison.InvariantCultureIgnoreCase))
                        {
                            //Relative path replaces only part of the query string
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
            // if not redirecting, go to next request delegate so routing can occur.
            if (!redirect)
                await next(context);
        }

        // retrieves the cache for the first time, begins the timer for the caching service
        void StartCachingRefreshService()
        {
            RefreshCache();
            //Timer to update the cache periodically
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
                var options = new RestClientOptions(baseUrl);

                // Ignore certificate errors if debugging
                if (Debugger.IsAttached)
                {
                    options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                }

                RestClient client = new RestClient(options);
                var redirects = client.Get<RedirectRule[]>(new RestRequest(apiPath));
                _cache.Set("_apiRedirectCache", redirects);
                _logger?.LogInformation($"Redirect cache refreshed successfully as {DateTime.Now.ToString()}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(101, ex, $"Unable to reach api at {baseUrl + apiPath}");
            }
        }

        // Loads the options from the application.json file
        void LoadOptions()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            baseUrl = config.GetValue<string>("AppSettings:RedirectByAPI.baseUrl");
            apiPath = config.GetValue<string>("AppSettings:RedirectByAPI.apiPath");
            cacheTimeout = config.GetValue<double>("AppSettings:RedirectByAPI.cacheTimeout");
        }

        // Creates a uri builder to assist in redirecting
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
}
