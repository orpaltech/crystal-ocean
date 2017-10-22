using System;

namespace CrystalOcean.ImageWebApi.Configuration
{
    public class JwtSecuritySettings
    {
        public String Key { get; set; }

        public String Issuer { get; set; }

        public String Audience { get; set; }

        public String Authority { get; set; }

        public int ExpiresIn { get; set; }
    }
}