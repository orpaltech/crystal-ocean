using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrystalOcean.Data.Models
{
    [Table("UserToken")]
    public class UserToken
    {
        [Key]
        public String Id { get; set; }

        public long UserId { get; set; }

        public bool Expired { get; set; }
    }
}