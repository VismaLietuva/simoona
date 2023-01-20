using Shrooms.Contracts.Enums;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Presentation.WebViewModels.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes.Events
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidateRemindersCollectionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is not IEnumerable<EventReminderViewModel>)
            {
                throw new ArgumentException($"Property is of type {value.GetType()} excepted {typeof(IEnumerable<EventReminderDto>)}");
            }

            var reminders = ((IEnumerable<EventReminderViewModel>)value).ToList();
            var remindTypes = Enum.GetValues(typeof(EventRemindType));
            if (remindTypes.Length < reminders.Count)
            {
                return new ValidationResult($"Expected reminders count to be less than or equal to {remindTypes.Length}." +
                    $"Received {reminders.Count}");
            }

            var receivedRemindersTypes = reminders.Select(reminder => reminder.Type).ToList();
            if (receivedRemindersTypes.Distinct().Count() != receivedRemindersTypes.Count)
            {
                return new ValidationResult($"Duplicate remind types are not allowed");
            }

            if (receivedRemindersTypes.Any(type => !ContainsType(remindTypes, type)))
            {
                return new ValidationResult($"Invalid remind type found");
            }

            if (reminders.Any(reminder => reminder.RemindBeforeInDays < 1))
            {
                return new ValidationResult($"Specified reminder in days cannot be negative or zero");
            }

            return ValidationResult.Success;
        }

        private static bool ContainsType(Array array, EventRemindType type)
        {
            foreach (var item in array)
            {
                if ((EventRemindType)item == type)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
