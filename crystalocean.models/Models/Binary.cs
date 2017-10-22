using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace CrystalOcean.Data.Models
{
    public abstract class Binary
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public uint ObjectId { get; set;}

        public String Checksum { get; set;}
    }
}
