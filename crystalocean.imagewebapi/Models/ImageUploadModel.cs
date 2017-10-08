using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CrystalOcean.ImageWebApi.Models
{
    public class ImageUploadModel /*: IValidatableObject*/
    {
        [Required]
        public String Name { get; set; }

        public String Description { get; set; }
/* 
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (...)
            {
                yield return new ValidationResult("Image name is required", new[] { "ProductId" });
            }
        }*/
    }
}