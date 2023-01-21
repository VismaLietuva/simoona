using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Presentation.WebViewModels.Events;
using Shrooms.Tests.Extensions;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.Tests.Controllers.ViewModels
{
    [TestFixture]
    public class CreateEventViewModelTests
    {
        [Test]
        public void NewInstance_ValidValues_ReturnsTrue()
        {
            var model = CreateValid();
            model.Reminders = new List<EventReminderViewModel>
            {
                new EventReminderViewModel
                {
                    Type = EventRemindType.Start,
                    RemindBeforeInDays = 1
                }
            };

            Assert.IsTrue(model.IsValid());
        }

        [Test]
        public void NewInstance_ZeroRemindNumber_ReturnsFalse()
        {
            var model = CreateValid();
            model.Reminders = new List<EventReminderViewModel>
            {
                new EventReminderViewModel
                {
                    Type = EventRemindType.Start,
                    RemindBeforeInDays = 0
                }
            };

            Assert.IsFalse(model.IsValid());
        }

        [Test]
        public void NewInstance_NegativeRemindNumber_ReturnsFalse()
        {
            var model = CreateValid();
            model.Reminders = new List<EventReminderViewModel>
            {
                new EventReminderViewModel
                {
                    Type = EventRemindType.Start,
                    RemindBeforeInDays = -10
                }
            };

            Assert.IsFalse(model.IsValid());
        }

        [Test]
        public void NewInstance_ReminderInvalidType_ReturnsFalse()
        {
            var model = CreateValid();
            model.Reminders = new List<EventReminderViewModel>
            {
                new EventReminderViewModel
                {
                    Type = (EventRemindType)int.MaxValue
                },
            };

            Assert.IsFalse(model.IsValid());
        }

        [Test]
        public void NewInstance_TooManyReminders_ReturnsFalse()
        {
            var model = CreateValid();
            model.Reminders = new List<EventReminderViewModel>
            {
                new EventReminderViewModel
                {
                    Type = EventRemindType.Start
                },
                new EventReminderViewModel
                {
                    Type = EventRemindType.Deadline
                },
                new EventReminderViewModel
                {
                    Type = (EventRemindType)int.MaxValue
                },
            };

            Assert.IsFalse(model.IsValid());
        }

        [Test]
        public void NewInstance_DuplicateReminderTypes_ReturnsFalse()
        {
            var model = CreateValid();
            model.Reminders = new List<EventReminderViewModel>
            {
                new EventReminderViewModel
                {
                    Type = EventRemindType.Start
                },
                new EventReminderViewModel
                {
                    Type = EventRemindType.Start
                }
            };

            Assert.IsFalse(model.IsValid());
        }

        [Test]
        public void NewInstance_DifferentReminderTypes_ReturnsTrue()
        {
            var model = CreateValid();
            model.Reminders = new List<EventReminderViewModel>
            {
                new EventReminderViewModel
                {
                    Type = EventRemindType.Deadline,
                    RemindBeforeInDays = 1,
                },
                new EventReminderViewModel
                {
                    Type = EventRemindType.Start,
                    RemindBeforeInDays = 1
                }
            };

            Assert.IsTrue(model.IsValid());
        }

        [Test]
        public void NewInstance_CreatingOneTimeEventAndHasReminders_ReturnsTrue()
        {
            var model = CreateValid();
            model.Reminders = new List<EventReminderViewModel>();

            Assert.IsTrue(model.IsValid());
        }

        [Test]
        public void NewInstance_CreatingRecurringEventAndHasReminders_ReturnsFalse()
        {
            var model = CreateValid(EventRecurrenceOptions.EveryWeek);
            model.Reminders = new List<EventReminderViewModel>();

            Assert.IsFalse(model.IsValid());
        }

        private static CreateEventViewModel CreateValid(EventRecurrenceOptions recurrence = EventRecurrenceOptions.None)
        {
            var startDate = DateTime.UtcNow;
            return new CreateEventViewModel
            {
                Name = "Event",
                ImageName = "event-image-id",
                StartDate = startDate,
                EndDate = startDate.AddDays(20),
                RegistrationDeadlineDate = startDate,
                Recurrence = recurrence,
                Offices = new List<int>(),
                Location = "new york",
                MaxParticipants = EventsConstants.EventMaxParticipants,
                MaxVirtualParticipants = EventsConstants.EventMaxParticipants,
                ResponsibleUserId = "user-id"
            };
        }
    }
}
