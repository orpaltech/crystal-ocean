using System;
using System.ComponentModel.DataAnnotations;

namespace CrystalOcean.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public String Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me on this computer")]
        public bool RememberMe { get; set; }
    }
}