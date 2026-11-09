using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    public class EventWaitlistValidationTests
    {
        private EventValidationService _sut;

        [SetUp]
        public void TestInitializer()
        {
            _sut = new EventValidationService(Substitute.For<ISystemClock>());
        }

        [Test]
        public void Should_Allow_Waitlist_Without_Sign_Up_Options()
        {
            Assert.DoesNotThrow(() => _sut.CheckIfWaitlistAllowed(true, 0, 0));
        }

        [Test]
        public void Should_Allow_Sign_Up_Options_Without_Waitlist()
        {
            Assert.DoesNotThrow(() => _sut.CheckIfWaitlistAllowed(false, 2, 3));
        }

        [TestCase(2, 0)]
        [TestCase(0, 1)]
        public void Should_Reject_Waitlist_Together_With_Sign_Up_Options(int legacyOptions, int questions)
        {
            var exception = Assert.Throws<EventException>(() => _sut.CheckIfWaitlistAllowed(true, legacyOptions, questions));

            Assert.That(exception.Message, Is.EqualTo(PremiumErrorCodes.EventWaitlistNotAllowedWithSignUpOptions));
        }

        [Test]
        public void Should_Reject_Disabling_Waitlist_While_People_Are_Queued()
        {
            var @event = new Event
            {
                HasWaitlist = true,
                EventParticipants = new List<EventParticipant>
                {
                    new EventParticipant { AttendStatus = (int)AttendingStatus.Waitlisted }
                }
            };

            var exception = Assert.Throws<EventException>(() => _sut.CheckIfWaitlistCanBeDisabled(false, @event));

            Assert.That(exception.Message, Is.EqualTo(PremiumErrorCodes.EventWaitlistCannotBeDisabledWithQueue));
        }

        [Test]
        public void Should_Allow_Disabling_Waitlist_When_Nobody_Is_Queued()
        {
            var @event = new Event
            {
                HasWaitlist = true,
                EventParticipants = new List<EventParticipant>
                {
                    new EventParticipant { AttendStatus = (int)AttendingStatus.Attending }
                }
            };

            Assert.DoesNotThrow(() => _sut.CheckIfWaitlistCanBeDisabled(false, @event));
        }

        [Test]
        public void Should_Allow_Keeping_Waitlist_On_While_People_Are_Queued()
        {
            var @event = new Event
            {
                HasWaitlist = true,
                EventParticipants = new List<EventParticipant>
                {
                    new EventParticipant { AttendStatus = (int)AttendingStatus.Waitlisted }
                }
            };

            Assert.DoesNotThrow(() => _sut.CheckIfWaitlistCanBeDisabled(true, @event));
        }

        [Test]
        public void Should_Not_Let_A_Client_Set_The_Waitlisted_Status_Directly()
        {
            var exception = Assert.Throws<EventException>(() =>
                _sut.CheckIfAttendOptionIsAllowed(AttendingStatus.Waitlisted, new EventJoinValidationDto()));

            Assert.That(exception.Message, Is.EqualTo(PremiumErrorCodes.EventAttendTypeIsNotAllowed));
        }
    }
}
