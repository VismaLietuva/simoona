using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Domain.Services.Users;
using Shrooms.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    [TestFixture]
    public class UserEventsServiceTests
    {
        private Organization _defaultOrganization;

        private UserEventsService _sut;

        private DbSet<EventReminder> _eventRemindersDbSet;

        private IUnitOfWork2 _uow;
        private ISystemClock _systemClock;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _eventRemindersDbSet = _uow.MockDbSetForAsync<EventReminder>();

            _systemClock = Substitute.For<ISystemClock>();
            _systemClock.UtcNow.Returns(DateTime.UtcNow);

            _defaultOrganization = new Organization
            {
                Id = 1
            };

            _sut = new UserEventsService(_uow, _systemClock);
        }

        [TestCase(EventReminderType.Start)]
        [TestCase(EventReminderType.Deadline)]
        public async Task GetReadyNotCompletedRemindersAsync_RemindersAvailableAndRemindersAreReady_ReturnsReminders(EventReminderType type)
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(5);
            var deadlineDate = startDate;
            var reminders = new List<EventReminder>
            {
                new EventReminder
                {
                    IsReminded = false,
                    RemindBeforeInDays = 10,
                    Type = type
                },
                new EventReminder
                {
                    IsReminded = false,
                    RemindBeforeInDays = 1,
                    Type = type
                },
                new EventReminder
                {
                    IsReminded = true,
                    RemindBeforeInDays = 10,
                    Type = type
                },
            };
            CreateEventWithRemindersForGetReadyNotCompletedRemindersAsyncTest(startDate, deadlineDate, reminders);

            // Act
            var result = await _sut.GetReadyNotCompletedRemindersAsync(_defaultOrganization);

            // Assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public async Task GetReadyNotCompletedRemindersAsync_OnlyRemindedRemindersExist_ReturnsEmptyCollection()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(5);
            var deadlineDate = startDate;
            var reminders = new List<EventReminder>
            {
                new EventReminder
                {
                    IsReminded = true,
                    RemindBeforeInDays = 10,
                    Type = EventReminderType.Start
                },
                new EventReminder
                {
                    IsReminded = true,
                    RemindBeforeInDays = 10,
                    Type = EventReminderType.Deadline
                },
            };
            CreateEventWithRemindersForGetReadyNotCompletedRemindersAsyncTest(startDate, deadlineDate, reminders);

            // Act
            var result = await _sut.GetReadyNotCompletedRemindersAsync(_defaultOrganization);

            // Assert
            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetReadyNotCompletedRemindersAsync_NoReadyRemindersAvailable_ReturnsEmptyCollection()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(5);
            var deadlineDate = startDate;
            var reminders = new List<EventReminder>
            {
                new EventReminder
                {
                    IsReminded = false,
                    RemindBeforeInDays = 1,
                    Type = EventReminderType.Start
                },
                new EventReminder
                {
                    IsReminded = false,
                    RemindBeforeInDays = 1,
                    Type = EventReminderType.Deadline
                },
            };
            CreateEventWithRemindersForGetReadyNotCompletedRemindersAsyncTest(startDate, deadlineDate, reminders);

            // Act
            var result = await _sut.GetReadyNotCompletedRemindersAsync(_defaultOrganization);

            // Assert
            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetReadyNotCompletedRemindersAsync_RemindersAvailableWithExpiredEventDates_ReturnsEmptyCollection()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-5);
            var deadlineDate = startDate;
            var reminders = new List<EventReminder>
            {
                new EventReminder
                {
                    IsReminded = false,
                    RemindBeforeInDays = 100,
                    Type = EventReminderType.Start
                },
                new EventReminder
                {
                    IsReminded = false,
                    RemindBeforeInDays = 100,
                    Type = EventReminderType.Deadline
                },
            };
            CreateEventWithRemindersForGetReadyNotCompletedRemindersAsyncTest(startDate, deadlineDate, reminders);

            // Act
            var result = await _sut.GetReadyNotCompletedRemindersAsync(_defaultOrganization);

            // Assert
            Assert.IsFalse(result.Any());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public async Task GetReadyNotCompletedRemindersAsync_RetrievesRemindersWithGivenOrganizationId_ReturnsReminders(int organizationId)
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(5);
            var deadlineDate = startDate;
            var reminders = new List<EventReminder>
            {
                new EventReminder
                {
                    IsReminded = false,
                    RemindBeforeInDays = 10,
                    Type = EventReminderType.Start
                },
                new EventReminder
                {
                    IsReminded = false,
                    RemindBeforeInDays = 1,
                    Type = EventReminderType.Start
                },
                new EventReminder
                {
                    IsReminded = true,
                    RemindBeforeInDays = 10,
                    Type = EventReminderType.Start
                },
            };
            CreateEventWithRemindersForGetReadyNotCompletedRemindersAsyncTest(startDate, deadlineDate, reminders, organizationId);

            // Act
            var result = await _sut.GetReadyNotCompletedRemindersAsync(new Organization { Id = organizationId });

            // Assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public async Task SetRemindersAsCompleteAsync_WhenEmptyRemindersCollection_DoesNotSaveChanges()
        {
            // Arrange
            var reminders = new List<EventReminder>();

            // Act
            await _sut.SetRemindersAsCompleteAsync(reminders);

            // Assert
            await _uow.Received(0).SaveChangesAsync(Arg.Any<bool>());
        }

        [Test]
        public async Task SetRemindersAsCompleteAsync_HasReminders_UpdatesRemindersStates()
        {
            // Arrange
            var reminder = new EventReminder
            {
                IsReminded = false,
                RemindedCount = 0
            };
            var reminders = new List<EventReminder> { reminder };

            // Act
            await _sut.SetRemindersAsCompleteAsync(reminders);

            // Assert
            Assert.IsTrue(reminder.IsReminded);
            Assert.AreEqual(1, reminder.RemindedCount);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<bool>());
        }

        private void CreateEventWithRemindersForGetReadyNotCompletedRemindersAsyncTest(
            DateTime eventStartDate,
            DateTime eventDeadlineDate,
            List<EventReminder> reminders,
            int? organizationId = null)
        {
            var @event = new Event
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId ?? _defaultOrganization.Id,
                EventParticipants = new List<EventParticipant>(),
                StartDate = eventStartDate,
                RegistrationDeadline = eventDeadlineDate,
            };

            foreach (var reminder in reminders)
            {
                reminder.Event = @event;
                reminder.EventId = @event.Id;
            }

            _eventRemindersDbSet.SetDbSetDataForAsync(reminders);
        }
    }
}
