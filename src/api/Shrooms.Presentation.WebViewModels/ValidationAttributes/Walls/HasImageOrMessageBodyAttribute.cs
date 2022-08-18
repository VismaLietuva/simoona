using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.ValidationAttributes.Walls
{
    /// <summary>
    /// Supports only IEnumerable<string>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HasImageOrMessageBodyAttribute : ValidationAttribute
    {
        private readonly string _messageBodyProperty;

        public HasImageOrMessageBodyAttribute(string messageBodyProperty)
        {
            _messageBodyProperty = messageBodyProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!TryGetMessageBodyValue(validationContext, out var messageBody))
            {
                throw new ArgumentException($"Provided property is not of type {typeof(string)}");
            }

            if (!TryGetImagesValue(value, out var images))
            {
                throw new ArgumentException($"Provided property is not of type {typeof(IEnumerable<string>)}");
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

        private bool TryGetMessageBodyValue(ValidationContext validationContext, out string messageBody)
        {
            messageBody = null;

            var messageBodyPropertyInfo = validationContext.ObjectType.GetProperty(_messageBodyProperty);

            if (messageBodyPropertyInfo == null)
            {
                return false;
            }

            var messageBodyObject = messageBodyPropertyInfo.GetValue(validationContext.ObjectInstance);

            if (messageBodyObject == null) // Cannot get type when object is null
            {
                return true;
            }

            if (messageBodyObject is not string)
            {
                return false;
            }

            messageBody = messageBodyObject as string;

            return true;
        }
    }
}
