namespace ASPNetCoreChallenge.Models
{
    public class RedirectRule
    {
        public string redirectUrl { get; set; }
        public string targetUrl { get; set; }
        public int redirectType { get; set; }
        public bool useRelative { get; set; }

        public bool permanent {get{
            return redirectType == 301;
        }}
    }
}
