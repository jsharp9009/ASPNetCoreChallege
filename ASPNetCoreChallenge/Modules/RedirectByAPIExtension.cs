using ASPNetCoreChallenge.Interfaces;

namespace ASPNetCoreChallenge.Modules
{
    /// <summary>
    /// Extension method for adding API Routing to the applicaiton
    /// </summary>
    public static class RedirectByAPIExtension
    {
        /// <summary>
        ///  Extension method for adding API Routing to the applicaiton
        /// </summary>
        /// <param name="app"></param>
        public static void UseAPIRouting(this WebApplication app)
        {
            IRedirectByAPI? redirectByAPI = app.Services.GetService<IRedirectByAPI>();
            if (redirectByAPI != null)
            {
                redirectByAPI.Attach(app);
            }
        }

        public static void AddRedirectAPIService(this IServiceCollection Services){
            //Register as singleton so we only have 1 instance of the class
            Services.AddSingleton<IRedirectByAPI, RedirectByAPI>();
        }
    }
}