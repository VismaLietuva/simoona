using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.Presentation.WebViewModels.Events;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Tests.ValidationAttributes
{
    [TestFixture]
    public class ValidateRemindersCollectionAttributeTests
    {
        private class Mock
        {
            public List<int> Numbers { get; set; }

            public List<EventReminderViewModel> Reminders { get; set; } 
        }

        [Test]
        public void IsValid_ValueIsNull_ReturnsSuccess()
        {
            // Arrange
            var model = new Mock { Reminders = null };
            var context = new ValidationContext(model);
            var attribute = new ValidateRemindersCollectionAttribute();

            // Act
            var result = attribute.GetValidationResult(model.Reminders, context);

            // Assert
            Assert.AreEqual(ValidationResult.Success, result);
        }

        [Test]
        public void IsValid_ValueIsNotEventReminderViewModelCollection_Throws()
        {
            // Arrange
            var model = new Mock { Numbers = new List<int>() };
            var context = new ValidationContext(model);
            var attribute = new ValidateRemindersCollectionAttribute();

            // Assert
            Assert.Throws<ArgumentException>(() => attribute.GetValidationResult(model.Numbers, context));
        }

        [Test]
        public void IsValid_MoreEntriesThanNecessary_ReturnsError()
        {
            // Arrange
            var reminders = new List<EventReminderViewModel>();
            for (var i = 0; i < Enum.GetValues(typeof(EventReminderType)).Length + 1; i++)
            {
                reminders.Add(new EventReminderViewModel());
            }
            var model = new Mock { Reminders = reminders };
            var context = new ValidationContext(model);
            var attribute = new ValidateRemindersCollectionAttribute();

            // Act
            var result = attribute.GetValidationResult(model.Reminders, context);

            // Assert
            Assert.IsNotNull(result.ErrorMessage);
        }

        [Test]
        public void IsValid_ContainsDuplicateTypes_ReturnsError()
        {
            // Arrange
            var reminders = new List<EventReminderViewModel>
            {
                new EventReminderViewModel
                {
                    Type = EventReminderType.Start
                },
                new EventReminderViewModel
                {
                    Type = EventReminderType.Start
                }
            };
            var model = new Mock { Reminders = reminders };
            var context = new ValidationContext(model);
            var attribute = new ValidateRemindersCollectionAttribute();

            // Act
            var result = attribute.GetValidationResult(model.Reminders, context);

            // Assert
            Assert.IsNotNull(result.ErrorMessage);
        }

        [Test]
        public void IsValid_ContainsIncorrectType_ReturnsError()
        {
            // Arrange
            var reminders = new List<EventReminderViewModel>
            {
                new EventReminderViewModel
                {
                    Type = EventReminderType.Start
                },
                new EventReminderViewModel
                {
                    Type = (EventReminderType)int.MaxValue
                }
            };
            var model = new Mock { Reminders = reminders };
            var context = new ValidationContext(model);
            var attribute = new ValidateRemindersCollectionAttribute();

            // Act
            var result = attribute.GetValidationResult(model.Reminders, context);

            // Assert
            Assert.IsNotNull(result.ErrorMessage);
        }

        [Test]
        public void IsValid_HasReminderWithRemindBeforeInDaysLessThanMinimumValue_ReturnsError()
        {
            // Arrange
            var reminders = new List<EventReminderViewModel>
            {
                new EventReminderViewModel
                {
                    Type = EventReminderType.Start,
                    RemindBeforeInDays = ValidationConstants.EventReminderRemindBeforeInDaysMin - 1
                },
                new EventReminderViewModel
                {
                    Type = EventReminderType.Start
                }
            };
            var model = new Mock { Reminders = reminders };
            var context = new ValidationContext(model);
            var attribute = new ValidateRemindersCollectionAttribute();

            // Act
            var result = attribute.GetValidationResult(model.Reminders, context);

            // Assert
            Assert.IsNotNull(result.ErrorMessage);
        }

        [Test]
        public void IsValid_HasReminderWithRemindBeforeInDaysGreaterThanMaximumValue_ReturnsError()
        {
            // Arrange
            var reminders = new List<EventReminderViewModel>
            {
                new EventReminderViewModel
                {
                    Type = EventReminderType.Start,
                    RemindBeforeInDays = ValidationConstants.EventReminderRemindBeforeInDaysMax + 1
                },
                new EventReminderViewModel
                {
                    Type = EventReminderType.Start
                }
            };
            var model = new Mock { Reminders = reminders };
            var context = new ValidationContext(model);
            var attribute = new ValidateRemindersCollectionAttribute();

            // Act
            var result = attribute.GetValidationResult(model.Reminders, context);

            // Assert
            Assert.IsNotNull(result.ErrorMessage);
        }

        [Test]
        public void IsValid_ValidValues_ReturnsSuccess()
        {
            // Arrange
            var reminders = new List<EventReminderViewModel>
            {
                new EventReminderViewModel
                {
                    Type = EventReminderType.Deadline,
                    RemindBeforeInDays = ValidationConstants.EventReminderRemindBeforeInDaysMax
                },
                new EventReminderViewModel
                {
                    Type = EventReminderType.Start,
                    RemindBeforeInDays = ValidationConstants.EventReminderRemindBeforeInDaysMin
                }
            };
            var model = new Mock { Reminders = reminders };
            var context = new ValidationContext(model);
            var attribute = new ValidateRemindersCollectionAttribute();

            // Act
            var result = attribute.GetValidationResult(model.Reminders, context);

            // Assert
            Assert.AreEqual(ValidationResult.Success, result);
        }
    }
}
