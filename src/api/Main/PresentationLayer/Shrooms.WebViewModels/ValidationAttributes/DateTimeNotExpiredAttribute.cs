using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DateTimeNotExpiredAttribute : ValidationAttribute
    {
        public DateTime DateTime
        {
            get;
            private set;
        }

        public override bool IsValid(object value)
        {
            DateTime = (DateTime)value;
            var result = DateTime > DateTime.UtcNow;
            return result;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"Provided {name} is expired ({DateTime})";
        }
    }
}
