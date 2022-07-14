using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTimeLessThanPresentDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || ((DateTime)value) <= DateTime.UtcNow)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Provided DateTime is in the future");
        }
    }
}
