namespace MVC.Services
{
    public class IdentityServerSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string DiscoveryUrl { get; set; }
    }
}