using System;
using System.ComponentModel.DataAnnotations;

namespace CrystalOcean.Data.Models
{
    public class Role
    {
        [Key]
        public long Id { get; set; } 
        public String Name { get; set; }
    }
}