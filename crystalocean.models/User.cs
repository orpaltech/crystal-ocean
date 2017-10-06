using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CrystalOcean.Data.Models
{
    [Table("User")]
    public class User : IdentityUser<long>
    {
        public String FirstName { get; set; }

        public String LastName { get; set; }
        
        public String MiddleName { get; set; }
        
        public String Prefix { get; set; }
        
        public String Gender { get; set; }

    }
}
