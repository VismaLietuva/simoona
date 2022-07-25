using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class DateTimeLessThanDateTimeAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateTimeLessThanDateTimeAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            ErrorMessage = ErrorMessageString;

            CheckIfValueIsTypeOfDateTime(value);

            var consumerDateTime = (DateTime)value;

            var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (comparisonProperty == null)
            {
                throw new ArgumentException($"Provided comparison property is not found");
            }

            var comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance);

            CheckIfValueIsTypeOfDateTime(comparisonValue);

            var comparisonDateTime = (DateTime)comparisonValue;

            if (consumerDateTime > comparisonDateTime)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }

        private static void CheckIfValueIsTypeOfDateTime(object value)
        {
            var type = value.GetType();

            if (type != typeof(DateTime))
            {
                throw new ArgumentException($"Provided property is not of type DateTime but of {type}");
            }
        }
    }
}
