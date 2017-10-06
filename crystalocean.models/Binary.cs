using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrystalOcean.Data.Models
{
    [Table("Binary")]
    public class Binary
    {
        [Key]
        public long Id { get; set; }

        public User User { get; set; }

        public uint ObjectId { get; set;}

        public String Checksum { get; set;}

        [NotMapped]
        public String FileName { get; set;}
    }
}
