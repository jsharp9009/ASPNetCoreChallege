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
        private readonly IMemoryCache _cache;

        public RedirectByAPI(IMemoryCache cache)
        {
            _cache = cache;
        }
               

        public void Attach(WebApplication app)
        {
            app.Use(AttachRouting);
            StartCachingRefreshService();
        }

        async Task AttachRouting(HttpContext context, RequestDelegate next)
        {            
            var path = context.Request.Path.ToString();
            IEnumerable<RedirectRule> redirectRules = null;
            bool redirect = false;
            if (_cache.TryGetValue("_apiRedirectCache", out redirectRules))
            {
                foreach (var rule in redirectRules)
                {
                    if (rule.useRelative)
                    {
                        if (path.Contains(rule.redirectUrl, StringComparison.InvariantCultureIgnoreCase))
                        {
                            path.Replace(rule.redirectUrl, rule.targetUrl, StringComparison.InvariantCultureIgnoreCase);
                            context.Response.Redirect(path, rule.redirectType == 301);
                            redirect = true;
                        }
                    }
                    else
                    {
                        if (path.Equals(rule.redirectUrl, StringComparison.InvariantCultureIgnoreCase))
                        {
                            context.Response.Redirect(rule.redirectUrl, rule.redirectType == 301);
                            redirect = true;
                        }
                    }
                }
            }
            if(!redirect)
                await next(context);
        }

        void StartCachingRefreshService()
        {
            RefreshCache();
            var timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 2 * 60000;
            timer.AutoReset= true; 
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
                RestClient client = new RestClient("https://localhost:7140/");
                var redirects = client.Get<RedirectRule[]>(new RestRequest("api/redirects"));
                _cache.Set("_apiRedirectCache", redirects);
            }
            catch { }
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
