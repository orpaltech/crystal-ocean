using System;
using System.ComponentModel.DataAnnotations;

namespace CrystalOcean.Web.Models
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public String Email { get; set; }
    }
}