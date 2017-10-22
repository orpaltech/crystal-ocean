using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace CrystalOcean.Data.Models
{
    public class Image : Binary
    {
        public String Format { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public int Rotation { get; set;}
    }
}