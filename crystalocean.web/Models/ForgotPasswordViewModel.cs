using System;
using System.ComponentModel.DataAnnotations;

namespace CrystalOcean.Web.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public String Email { get; set; }        
    }
}