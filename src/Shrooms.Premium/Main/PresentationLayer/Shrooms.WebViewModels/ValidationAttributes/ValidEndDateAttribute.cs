using Shrooms.WebViewModels.Models.Events;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.ValidationAttributes
{
    public class ValidEndDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (CreateEventViewModel)validationContext.ObjectInstance;
            DateTime startDate = Convert.ToDateTime(model.StartDate);
            DateTime endDate = Convert.ToDateTime(value);

            if (endDate < startDate)
            {
                return new ValidationResult("EndDate can not be less than start date");
            }
            return ValidationResult.Success;
        }
    }
}
