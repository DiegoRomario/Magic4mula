namespace M4.Infrastructure.Configurations.Models
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public int ExpirationTimeInHours { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }

    }
}
