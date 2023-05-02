namespace ASPNetCoreChallenge.Models
{
    public class RedirectRule
    {
        /// <summary>
        /// Url to check for
        /// </summary>
        public string redirectUrl { get; set; }
        /// <summary>
        /// Url to redirect redirectUrl is found
        /// </summary>
        public string targetUrl { get; set; }
        /// <summary>
        /// Http Status code. Used to determine if redirect is permanent or temorary
        /// </summary>
        public int redirectType { get; set; }
        /// <summary>
        /// Determines if we replace part of the url or redirect to a new page
        /// </summary>
        public bool useRelative { get; set; }
        /// <summary>
        /// Is redirect permanent?
        /// </summary>
        public bool permanent
        {
            get
            {
                //Thse redirect types are permanent, anything else will be temporary
                return redirectType == 301 || redirectType == 308;
            }
        }
    }
}
