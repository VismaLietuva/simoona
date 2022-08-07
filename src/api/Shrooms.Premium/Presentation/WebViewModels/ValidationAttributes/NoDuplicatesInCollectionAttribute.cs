using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NoDuplicatesInCollectionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is not ICollection)
            {
                throw new ArgumentException($"Property has to be of type {typeof(ICollection)}");
            }

            var collection = value as ICollection;

            if (collection.Count == 0)
            {
                return ValidationResult.Success;
            }

            var isCollectionItemsTypeChecked = false;
            var checkedItems = new HashSet<object>();

            foreach (var item in collection)
            {
                if (!isCollectionItemsTypeChecked)
                {
                    CheckIfItemIsOfValidType(item, out isCollectionItemsTypeChecked);
                }

                if (checkedItems.Contains(item))
                {
                    return new ValidationResult("No duplicates allowed");
                }

                checkedItems.Add(item);
            }

            return ValidationResult.Success;
        }

        private void CheckIfItemIsOfValidType(object item, out bool typeChecked)
        {
            var type = item.GetType();

            if (!type.IsValueType && !type.IsPrimitive && type != typeof(string))
            {
                throw new ArgumentException("Collection has to contain value types or strings");
            }

            typeChecked = true;
        }
    }
}
