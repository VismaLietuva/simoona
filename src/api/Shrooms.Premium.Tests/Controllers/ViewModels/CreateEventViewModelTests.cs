using NUnit.Framework;
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
        public void NewInstance_CreatingOneTimeEventAndHasRemindBeforeEventStartValueSet_ReturnsTrue()
        {
            var model = CreateValid();
            model.RemindBeforeEventStartInDays = 10;

            Assert.IsTrue(model.IsValid());
        }

        [Test]
        public void NewInstance_CreatingRecurringEventAndHasRemindBeforeEventStartValueSet_ReturnsFalse()
        {
            var model = CreateValid(EventRecurrenceOptions.EveryWeek);
            model.RemindBeforeEventStartInDays = 10;

            Assert.IsFalse(model.IsValid());
        }

        [Test]
        public void NewInstance_CreatingOneTimeEventAndHasRemindBeforeEventRegistrationDeadlineInDaysValueSet_ReturnsTrue()
        {
            var model = CreateValid();
            model.RemindBeforeEventRegistrationDeadlineInDays = 10;

            Assert.IsTrue(model.IsValid());
        }

        [Test]
        public void NewInstance_CreatingRecurringEventAndHasRemindBeforeEventRegistrationDeadlineInDaysValueSet_ReturnsFalse()
        {
            var model = CreateValid(EventRecurrenceOptions.EveryWeek);
            model.RemindBeforeEventRegistrationDeadlineInDays = 10;

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
                MaxParticipants = EventsConstants.EventMaxParticipants - 1,
                MaxVirtualParticipants = EventsConstants.EventMaxParticipants - 1,
                ResponsibleUserId = "user-id"
            };
        }
    }
}
