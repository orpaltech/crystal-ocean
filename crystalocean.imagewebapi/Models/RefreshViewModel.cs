using System;

namespace CrystalOcean.ImageWebApi.Models
{
    public class RefreshViewModel
    {
        public long UserId { get; set; }

        public String RefreshToken { get; set; }
    }
}