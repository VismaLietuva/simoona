using Shrooms.DataLayer.EntityModels.Models.Events;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes.Events
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequireOneTimeEventForCollectionAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public RequireOneTimeEventForCollectionAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var recurrenceOption = GetRecurrenceOption(validationContext);
            if (IsOneTimeEvent(recurrenceOption) ||
                value == null ||
                ((ICollection)value).Count == 0)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult($"Value {nameof(value)} can only be set on one time event");
        }

        private static bool IsOneTimeEvent(EventRecurrenceOptions recurrenceOption)
        {
            return recurrenceOption == EventRecurrenceOptions.None;
        }

        private EventRecurrenceOptions GetRecurrenceOption(ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            CheckIfPropertyFound(property);

            var propertyValue = property.GetValue(validationContext.ObjectInstance);
            CheckIfValueIsEventRecurrenceOption(propertyValue);

            return (EventRecurrenceOptions)propertyValue;
        }

        private static void CheckIfPropertyFound(System.Reflection.PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentException($"Provided comparison property is not found");
            }
        }

        private static void CheckIfValueIsEventRecurrenceOption(object propertyValue)
        {
            var type = propertyValue?.GetType();
            if (type != typeof(EventRecurrenceOptions))
            {
                throw new ArgumentException($"Provided property is not of type {nameof(EventRecurrenceOptions)} but of type {type}");
            }
        }
    }
}
