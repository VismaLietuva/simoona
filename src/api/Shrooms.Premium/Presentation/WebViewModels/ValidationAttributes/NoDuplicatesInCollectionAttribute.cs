using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes
{
    /// <summary>
    /// Can be used with collections that contain value types or strings.
    /// It can also be used on collections with reference types,
    /// but you would need to specify a property name that contains a value type or string
    /// (one level of nesting is supported).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NoDuplicatesInCollectionAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public NoDuplicatesInCollectionAttribute(string comparisonProperty = null)
        {
            _comparisonProperty = comparisonProperty;
        }

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

            var isCollectionItemTypeChecked = false;
            var checkedItems = new HashSet<object>();

            foreach (var item in collection)
            {
                if (!isCollectionItemTypeChecked)
                {
                    CheckIfCollectionItemIsOfValidType(item, out isCollectionItemTypeChecked);
                }

                var itemValue = GetItemValue(item);

                if (checkedItems.Contains(itemValue))
                {
                    return new ValidationResult("No duplicates allowed");
                }

                checkedItems.Add(itemValue);
            }

            return ValidationResult.Success;
        }

        private object GetItemValue(object item)
        {
            if (_comparisonProperty == null)
            {
                return item;
            }

            return item
                .GetType()
                .GetProperty(_comparisonProperty)
                .GetValue(item);
        }

        private void CheckIfCollectionItemIsOfValidType(object item, out bool typeChecked)
        {
            var type = item.GetType();

            if (_comparisonProperty != null)
            {
                type = type
                    .GetProperty(_comparisonProperty, BindingFlags.Instance | BindingFlags.Public)
                    .PropertyType;
            }

            if (!type.IsValueType && !type.IsPrimitive && type != typeof(string))
            {
                throw new ArgumentException("Collection has to contain value types or strings");
            }

            typeChecked = true;
        }
    }
}
