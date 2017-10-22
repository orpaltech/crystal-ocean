using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CrystalOcean.Data.Models
{
    [Table("Role")]
    public class Role : IdentityRole<long>
    {
        public static readonly String ADMIN = "Admin";
        public static readonly String USER = "User";

        public String Description { get; set; }
    }
}