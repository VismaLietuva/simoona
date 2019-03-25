using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DateTimeNotExpiredAttribute : ValidationAttribute
    {
        private DateTime _dateTime;

        public DateTime DateTime
        {
            get
            {
                return _dateTime;
            }
        }

        public override bool IsValid(object value)
        {
            _dateTime = (DateTime)value;
            bool result = DateTime > DateTime.UtcNow ? true : false;
            return result;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"Provided {name} is expired ({DateTime})";
        }
    }
}
