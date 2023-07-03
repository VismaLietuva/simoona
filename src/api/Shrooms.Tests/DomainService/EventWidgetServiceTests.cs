using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models.Events;
using System.Data.Entity;
using Shrooms.Tests.Extensions;
using Shrooms.Domain.Services.Events;
using Shrooms.Contracts.Infrastructure;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class EventWidgetServiceTests
    {
        private const int DefaultOrganizationId = 1;

        private DbSet<Event> _eventsDbSet;
        private ISystemClock _systemClock;

        private EventWidgetService _sut;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _systemClock = Substitute.For<ISystemClock>();
            _eventsDbSet = uow.MockDbSetForAsync<Event>();

            _sut = new EventWidgetService(uow, _systemClock);
        }

        [Test]
        public void GetUpcomingEventsAsync_WhenEventCountIsLessThanOne_Throws()
        {
            // Arrange
            const int eventCount = -1;

            // Assert
            Assert.ThrowsAsync<ArgumentException>(async () => await _sut.GetUpcomingEventsAsync(DefaultOrganizationId, eventCount));
        }

        [Test]
        public async Task GetUpcomingEventsAsync_WhenEventCountIsFive_ReturnsFiveEvents()
        {
            // Arrange
            const int eventCount = 5;
            var currentDate = DateTime.UtcNow;
            CreateEventsForGetUpcomingEventsAsyncTest(
                currentDate,
                currentDate,
                eventsWithUpcomingTypeSet: eventCount,
                eventCount: eventCount);
            _systemClock.UtcNow.Returns(currentDate.AddDays(-10));

            // Act
            var result = await _sut.GetUpcomingEventsAsync(DefaultOrganizationId, eventCount);

            // Assert
            Assert.AreEqual(eventCount, result.Count());
        }

        [Test]
        public async Task GetUpcomingEventsAsync_WhenAllEventsAreAlreadyStarted_ReturnsEmptyCollection()
        {
            // Arrange
            const int eventCount = 5;
            var currentDate = DateTime.UtcNow;
            CreateEventsForGetUpcomingEventsAsyncTest(currentDate, currentDate, eventsWithUpcomingTypeSet: eventCount, eventCount: eventCount);
            _systemClock.UtcNow.Returns(currentDate.AddDays(10));

            // Act
            var result = await _sut.GetUpcomingEventsAsync(DefaultOrganizationId, eventCount);

            // Assert
            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetUpcomingEventsAsync_WhenEventTypeDoesNotAllowEventToBeShownButAllEventsHaveIsShownInUpcomingEventsWidgetSetToTrue_ReturnsEmptyCollection()
        {
            // Arrange
            const int eventCount = 5;
            var currentDate = DateTime.UtcNow;
            var events = CreateEventsForGetUpcomingEventsAsyncTest(
                currentDate,
                currentDate,
                eventsWithUpcomingTypeSet: 0,
                eventCount: eventCount);
            events[0].IsShownInUpcomingEventsWidget = true;
            _eventsDbSet.SetDbSetDataForAsync(events);
            _systemClock.UtcNow.Returns(currentDate.AddDays(-10));

            // Act
            var result = await _sut.GetUpcomingEventsAsync(DefaultOrganizationId, eventCount);

            // Assert
            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetUpcomingEventsAsync_WhenEventTypeAllowsEventToBeShownButAllEventsHaveIsShownInUpcomingEventsWidgetSetToFalse_ReturnsEmptyCollection()
        {
            // Arrange
            const int eventCount = 1;
            var currentDate = DateTime.UtcNow;
            var events = CreateEventsForGetUpcomingEventsAsyncTest(
                currentDate,
                currentDate,
                eventsWithUpcomingTypeSet: eventCount,
                eventCount: eventCount);
            events[0].IsShownInUpcomingEventsWidget = false;
            _eventsDbSet.SetDbSetDataForAsync(events);
            _systemClock.UtcNow.Returns(currentDate.AddDays(-10));

            // Act
            var result = await _sut.GetUpcomingEventsAsync(DefaultOrganizationId, eventCount);

            // Assert
            Assert.IsFalse(result.Any());
        }

        [Test]
        public async Task GetUpcomingEventsAsync_WhenEventsAreNotStarted_ReturnsEventsOrderedInAscendingOrderByDeadlineDateThenByStartDate()
        {
            // Arrange
            const int eventCount = 5;
            var currentDate = DateTime.UtcNow;
            var eventStartDate = currentDate;
            var eventDeadlineDate = currentDate.AddDays(-20);
            CreateEventsForGetUpcomingEventsAsyncTest(
                eventStartDate,
                eventDeadlineDate,
                eventsWithUpcomingTypeSet: eventCount,
                eventCount: eventCount,
                additionalEvents: () => new List<Event>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = DefaultOrganizationId,
                        StartDate = currentDate.AddDays(-50),
                        RegistrationDeadline = currentDate.AddDays(-60),
                        EventType = new EventType
                        {
                            Name = Guid.NewGuid().ToString(),
                            CanBeDisplayedInUpcomingEventsWidget = true
                        }
                    }
                });
            _systemClock.UtcNow.Returns(currentDate.AddDays(-30));

            // Act
            var result = await _sut.GetUpcomingEventsAsync(DefaultOrganizationId, eventCount);

            // Assert
            CollectionAssert.AreEqual(result.OrderBy(x => x.RegistrationDeadlineDate).ThenBy(x => x.StartDate), result);
        }

        [Test]
        public async Task GetUpcomingEventsAsync_WhenEventsAreNotStartedAndTypeDoesNotAllowToShowEvent_ReturnsEventsThatCanBeShown()
        {
            // Arrange
            const int eventsWithUpcomingTypeSet = 3;
            var currentDate = DateTime.UtcNow;
            CreateEventsForGetUpcomingEventsAsyncTest(currentDate, currentDate, eventsWithUpcomingTypeSet);
            _systemClock.UtcNow.Returns(currentDate.AddDays(-10));

            // Act
            var result = await _sut.GetUpcomingEventsAsync(DefaultOrganizationId);

            // Assert
            Assert.AreEqual(eventsWithUpcomingTypeSet, result.Count());
        }

        public List<Event> CreateEventsForGetUpcomingEventsAsyncTest(
            DateTime eventStartDate,
            DateTime eventDeadlineDate,
            int eventsWithUpcomingTypeSet = 3,
            int eventCount = 5,
            Func<List<Event>> additionalEvents = null)
        {
            var events = new List<Event>();
            for (var i = 0; i < eventCount; i++)
            {
                events.Add(new Event
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = DefaultOrganizationId,
                    StartDate = eventStartDate,
                    RegistrationDeadline = eventDeadlineDate,
                    EventType = new EventType
                    {
                        Name = Guid.NewGuid().ToString(),
                        CanBeDisplayedInUpcomingEventsWidget = i < eventsWithUpcomingTypeSet
                    },
                    IsShownInUpcomingEventsWidget = true
                });
            }

            if (additionalEvents != null)
            {
                events.AddRange(additionalEvents());
            }

            _eventsDbSet.SetDbSetDataForAsync(events);
            return events;
        }
    }
}
