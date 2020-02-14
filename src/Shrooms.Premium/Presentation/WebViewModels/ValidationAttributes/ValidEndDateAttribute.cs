using System;
using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.Presentation.WebViewModels.Models.Events;

namespace Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes
{
    public class ValidEndDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (CreateEventViewModel)validationContext.ObjectInstance;
            var startDate = Convert.ToDateTime(model.StartDate);
            var endDate = Convert.ToDateTime(value);

            if (endDate < startDate)
            {
                return new ValidationResult("EndDate can not be less than start date");
            }
            return ValidationResult.Success;
        }
    }
}
