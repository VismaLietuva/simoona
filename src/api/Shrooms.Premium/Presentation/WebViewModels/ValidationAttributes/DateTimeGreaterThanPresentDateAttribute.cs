using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTimeGreaterThanPresentDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is not DateTime)
            {
                throw new ArgumentException("The property must be of type DateTime");
            }

            if (value is DateTime date && date > DateTime.UtcNow)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Provided DateTime is in the past");
        }
    }
}
