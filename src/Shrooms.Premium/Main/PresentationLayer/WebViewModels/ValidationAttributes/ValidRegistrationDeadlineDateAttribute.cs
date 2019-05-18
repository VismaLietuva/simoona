using System;
using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Events;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.ValidationAttributes
{
    public class ValidRegistrationDeadlineDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (CreateEventViewModel)validationContext.ObjectInstance;
            var startDate = Convert.ToDateTime(model.StartDate);
            var registrationDeadlineDate = Convert.ToDateTime(value);

            if (registrationDeadlineDate > startDate)
            {
                return new ValidationResult("RegistrationDeadlineDate can not be greater than start date");
            }
            return ValidationResult.Success;
        }
    }
}
