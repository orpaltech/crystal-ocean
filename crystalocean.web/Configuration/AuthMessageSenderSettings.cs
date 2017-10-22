using System;

namespace CrystalOcean.Web.Configuration
{
    public class AuthMessageSenderSettings
    {
        public String Host { get; set; }

        public int Port { get; set; }

        public String Username { get; set; }

        public String Password { get; set; }

        public String FromEmail { get; set; }

        public int Priority { get; set; }

        public bool UseSSL { get; set;}
    }
}