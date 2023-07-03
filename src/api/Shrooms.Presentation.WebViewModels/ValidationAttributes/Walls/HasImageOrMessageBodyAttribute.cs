using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.ValidationAttributes.Walls
{
    /// <summary>
    /// Supports only IEnumerable<string>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HasImageOrMessageBodyAttribute : ValidationAttribute // Can modify here
    {
        private readonly string _messageBodyProperty;
        private readonly string _pictureIdProperty;

        public HasImageOrMessageBodyAttribute(string messageBodyProperty, string pictureIdProperty)
        {
            _messageBodyProperty = messageBodyProperty;
            _pictureIdProperty = pictureIdProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!TryGetStringValueFromProperty(validationContext, _pictureIdProperty, out var pictureId))
            {
                throw new ArgumentException(GetErrorMessage(typeof(string)));
            }

            if (pictureId != null)
            {
                return ValidationResult.Success;
            }

            if (!TryGetStringValueFromProperty(validationContext, _messageBodyProperty, out var messageBody))
            {
                throw new ArgumentException(GetErrorMessage(typeof(string)));
            }

            if (!TryGetImagesValue(value, out var images))
            {
                throw new ArgumentException(GetErrorMessage(typeof(IEnumerable<string>)));
            }

            if (messageBody != null && images == null)
            {
                return ValidationResult.Success;
            }

            if (messageBody == null && images == null)
            {
                return new ValidationResult("Content has to contain either message or image");
            }

            var count = 0;

            foreach (var image in images)
            {
                if (image == null)
                {
                    return new ValidationResult("Images cannot contain null values");
                }

                count++;
            }

            if (messageBody == null && count == 0)
            {
                return new ValidationResult("Content has to contain either message or image");
            }

            return ValidationResult.Success;
        }

        private string GetErrorMessage(Type type)
        {
            return $"Provided property is not of type {type}";
        }

        private bool TryGetImagesValue(object value, out IEnumerable<string> images)
        {
            images = null;

            if (value == null) // Cannot get type when object is null
            {
                return true;
            }

            if (value is not IEnumerable<string>)
            {
                return false;
            }

            images = value as IEnumerable<string>;

            return true;
        }

        private bool TryGetStringValueFromProperty(ValidationContext validationContext, string property, out string s)
        {
            s = null;

            var propertyInfo = validationContext.ObjectType.GetProperty(property);

            if (propertyInfo == null)
            {
                return false;
            }

            var valueObject = propertyInfo.GetValue(validationContext.ObjectInstance);

            if (valueObject == null) // Cannot get type when object is null
            {
                return true;
            }

            if (valueObject is not string)
            {
                return false;
            }

            s = valueObject as string;

            return true;
        }
    }
}
