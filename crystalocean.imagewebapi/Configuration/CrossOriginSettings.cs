using System;
using System.Collections.Generic;

namespace CrystalOcean.ImageWebApi.Configuration
{
    public class CrossOriginSettings
    {
        public bool Enabled { get; set; }

        public String PolicyName { get; set; }

        public bool AllowAnyOrigin { get; set; }

        public bool AllowAnyHeader { get; set; }

        public bool AllowAnyMethod { get; set; }

        public IEnumerable<String> AllowedOrigins { get; set; }
    
        public IEnumerable<String> AllowedHeaders { get; set; }

        public IEnumerable<String> AllowedMethods { get; set; }
    }
}