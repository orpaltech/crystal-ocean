using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CrystalOcean.ImageWebApi.Models
{
    public class ImageUploadModel
    {
        public String Name { get; set; }

        public String Description { get; set; }

        [Required]
        public String Checksum { get; set; }
    }
}