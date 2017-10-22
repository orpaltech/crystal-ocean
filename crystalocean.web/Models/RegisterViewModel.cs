using System;
using System.ComponentModel.DataAnnotations;

namespace CrystalOcean.Web.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public String Email { get;set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public String Password { get; set;}

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public String ConfirmPassword { get; set;}

        public String FirstName { get;set; }

        public String LastName { get;set; }
        
        public String Prefix { get; set; }
        
        public String Gender { get; set; }
    }
}