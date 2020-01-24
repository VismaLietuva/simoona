using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.DataTransferObjects.Models;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Events.List;
using Shrooms.Premium.Main.BusinessLayer.DomainExceptions.Event;
using Shrooms.Premium.Main.BusinessLayer.DomainServiceValidators.Events;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.Premium.UnitTests.DomainService
{
    public class EventListingServiceTests
    {
        private IEventListingService _eventListingService;
        private IDbSet<Event> _eventsDbSet;
        private ISystemClock _systemClockMock;
        private EventValidationService _eventValidationService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _eventsDbSet = uow.MockDbSet<Event>();

            _systemClockMock = Substitute.For<ISystemClock>();
            _eventValidationService = new EventValidationService(_systemClockMock);
            var eventValidationService = Substitute.For<IEventValidationService>();

            _eventListingService = new EventListingService(uow, eventValidationService);
        }

        [Test]
        public void Should_Return_Events_List_By_Type_For_Creator_And_Participant()
        {
            var eventsGuids = MockEventsListTest();
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
                UserId = "responsibleUserId"
            };

            var result = _eventListingService.GetEventsByType(userOrg, 1).ToList();
            Assert.AreEqual(result.First().Id, eventsGuids[0]);
            Assert.IsTrue(result.First().IsCreator);
            Assert.AreEqual(result.First().ParticipantsCount, 2);
            Assert.AreEqual(result.First().ParticipatingStatus, 1);
        }

        [Test]
        public void Should_Return_Events_List_By_Type_For_User()
        {
            var eventsGuids = MockEventsListTest();
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
                UserId = "notParticipantOrCreatorId"
            };

            var result = _eventListingService.GetEventsByType(userOrg, 1).ToList();
            Assert.AreEqual(result.First().Id, eventsGuids[0]);
            Assert.IsFalse(result.First().IsCreator);
            Assert.AreEqual(result.First().ParticipantsCount, 2);
            Assert.AreEqual(result.First().ParticipatingStatus, 3);
        }

        [Test]
        public void Should_Return_All_Events()
        {
            var eventGuids = MockEventsListTest();
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
                UserId = "userId"
            };
            var result = _eventListingService.GetEventsByType(userOrg).ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(eventGuids[2], result.First().Id);
        }

        [Test]
        public void Should_Return_My_Events_As_A_Participant()
        {
            var eventGuids = MockEventsListTest();
            var myEventsOptions = new MyEventsOptionsDTO
            {
                OrganizationId = 2,
                UserId = "testUser1",
                SearchString = null,
                Filter = BusinessLayerConstants.MyEventsOptions.Participant
            };

            var result = _eventListingService.GetMyEvents(myEventsOptions).ToList();
            Assert.AreEqual(result.Count, 3);
            Assert.AreEqual(result.First(x => x.Id == eventGuids[0]).ParticipatingStatus, 1);
            Assert.IsTrue(result.First(x => x.Id == eventGuids[2]).StartDate < result.First(x => x.Id == eventGuids[0]).StartDate);
        }

        [Test]
        public void Should_Return_My_Events_As_A_Master()
        {
            var eventGuids = MockEventsListTest();
            var myEventsOptions = new MyEventsOptionsDTO
            {
                OrganizationId = 2,
                UserId = "responsibleUserId2",
                SearchString = null,
                Filter = BusinessLayerConstants.MyEventsOptions.Host
            };

            var result = _eventListingService.GetMyEvents(myEventsOptions).ToList();
            Assert.AreEqual(result.Count, 1);
            Assert.IsTrue(result.First(x => x.Id == eventGuids[3]).IsCreator);
        }

        [Test]
        public void Should_Return_Options_By_Event_Id()
        {
            var eventsGuids = MockEventOptionsWithEvents();
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };

            var result = _eventListingService.GetEventOptions(eventsGuids[1], userOrg);
            Assert.AreEqual(result.Options.Count(), 2);
            Assert.AreEqual(result.Options.First(o => o.Id == 4).Option, "Option1");
            Assert.AreEqual(result.Options.First(o => o.Id == 5).Option, "Option2");
        }

        [Test]
        public void Should_Throw_If_Event_Deadline_Is_Greater_Than_Start_Date()
        {
            var deadlineDate = DateTime.Parse("2016-05-01");
            var startDate = DateTime.Parse("2016-04-28");
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfRegistrationDeadlineExceedsStartDate(deadlineDate, startDate));
            Assert.AreEqual(ex.Message, PremiumErrorCodes.EventRegistrationDeadlineGreaterThanStartDateCode);
        }

        [Test]
        public void Should_Not_Throw_If_Event_Deadline_Is_Lesser_Than_Start_Date()
        {
            var deadlineDate = DateTime.Parse("2016-04-28");
            var startDate = DateTime.Parse("2016-04-29");
            Assert.DoesNotThrow(() => _eventValidationService.CheckIfRegistrationDeadlineExceedsStartDate(deadlineDate, startDate));
        }

        [Test]
        public void Should_Not_Throw_If_Event_Deadline_Is_Equal_To_Start_Date()
        {
            var deadlineDate = DateTime.Parse("2016-04-29");
            var startDate = DateTime.Parse("2016-04-29");
            Assert.DoesNotThrow(() => _eventValidationService.CheckIfRegistrationDeadlineExceedsStartDate(deadlineDate, startDate));
        }

        [Test]
        public void Should_Throw_If_Deadline_Date_Has_Passed()
        {
            var deadlineDate = DateTime.Parse("2016-05-01");
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-05-02"));
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfRegistrationDeadlineIsExpired(deadlineDate));
            Assert.AreEqual(ex.Message, PremiumErrorCodes.EventRegistrationDeadlineIsExpired);
        }

        [Test]
        public void Should_Not_Throw_If_Deadline_Date_Is_Valid()
        {
            var deadlineDate = DateTime.Parse("2016-05-01");
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-05-02"));
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfRegistrationDeadlineIsExpired(deadlineDate));
            Assert.AreEqual(ex.Message, PremiumErrorCodes.EventRegistrationDeadlineIsExpired);
        }

        #region Mocks
        private Guid[] MockEventOptionsWithEvents()
        {
            Guid[] guids = Enumerable.Repeat(0, 2).Select(x => Guid.NewGuid()).ToArray();


            var options1 = new List<EventOption>
            {
                new EventOption
                {
                    Id = 1,
                    EventId = guids[0],
                    Option = "Option1",
                },
                new EventOption
                {
                    Id = 2,
                    EventId = guids[0],
                    Option = "Option2",
                },
                new EventOption
                {
                    Id = 3,
                    EventId = guids[0],
                    Option = "Option3",
                }
            };

            var options2 = new List<EventOption>
            {
                new EventOption
                {
                    Id = 4,
                    EventId = guids[1],
                    Option = "Option1",
                },
                new EventOption
                {
                    Id = 5,
                    EventId = guids[1],
                    Option = "Option2",
                }
            };

            var events = new List<Event>
            {
                new Event
                {
                    Id = guids[0],
                    MaxChoices = 1,
                    MaxParticipants = 20,
                    OrganizationId = 2,
                    Name = "Test event",
                    EventType = new EventType
                    {
                        Name = "test type",
                        IsSingleJoin = false,
                        Id = 1
                    },
                    EventParticipants = new List<EventParticipant>(),
                    EventTypeId = 1,
                    EventOptions = options1
                },
                new Event
                {
                    Id = guids[1],
                    MaxChoices = 1,
                    MaxParticipants = 20,
                    OrganizationId = 2,
                    Name = "Test event",
                    EventType = new EventType
                    {
                        Name = "test type",
                        IsSingleJoin = false,
                        Id = 1
                    },
                    EventParticipants = new List<EventParticipant>(),
                    EventTypeId = 1,
                    EventOptions = options2
                }
            };
            _eventsDbSet.SetDbSetData(events.AsQueryable());
            return guids;
        }

        private Guid[] MockEventsListTest()
        {
            var guids = Enumerable.Repeat(0, 4).Select(x => Guid.NewGuid()).ToArray();

            var participant1 = new EventParticipant
            {
                ApplicationUserId = "responsibleUserId",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 1,
                EventId = guids[0],
                AttendStatus = 1
            };

            var participant2 = new EventParticipant
            {
                ApplicationUserId = "testUser1",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 2,
                EventId = guids[0],
                AttendStatus = 1
            };

            var participant3 = new EventParticipant
            {
                ApplicationUserId = "testUser2",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 3,
                EventId = guids[1],
                AttendStatus = 3
            };

            var events = new List<Event>
            {
                new Event
                {
                    Id = guids[0],
                    StartDate = DateTime.UtcNow.AddDays(4),
                    EndDate = DateTime.UtcNow.AddDays(4),
                    Created = DateTime.UtcNow,
                    EventTypeId = 1,
                    ResponsibleUserId = "responsibleUserId",
                    ImageName = "imageUrl",
                    Name = "Drinking event",
                    Place = "City",
                    MaxParticipants = 15,
                    OrganizationId = 2,
                    EventParticipants = new List<EventParticipant> { participant1, participant2 },
                    EventOptions = new List<EventOption>()
                },
                new Event
                {
                    Id = guids[1],
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    Created = DateTime.UtcNow,
                    EventTypeId = 2,
                    ResponsibleUserId = "responsibleUserId",
                    ImageName = "imageUrl",
                    Name = "Drinking event",
                    Place = "City",
                    MaxParticipants = 15,
                    OrganizationId = 3,
                    EventParticipants = new List<EventParticipant> { participant3 },
                    EventOptions = new List<EventOption>()
                },
                new Event
                {
                    Id = guids[2],
                    StartDate = DateTime.UtcNow.AddDays(3),
                    EndDate = DateTime.UtcNow.AddDays(3),
                    Created = DateTime.UtcNow,
                    EventTypeId = 3,
                    ResponsibleUserId = "responsibleUserId",
                    ImageName = "imageUrl",
                    Name = "Some event",
                    Place = "Some place",
                    MaxParticipants = 10,
                    OrganizationId = 2,
                    EventParticipants = new List<EventParticipant> { participant2 },
                    EventOptions = new List<EventOption>()
                },
                new Event
                {
                    Id = guids[3],
                    StartDate = DateTime.UtcNow.AddDays(-3),
                    EndDate = DateTime.UtcNow.AddDays(-3),
                    Created = DateTime.UtcNow,
                    EventTypeId = 3,
                    ResponsibleUserId = "responsibleUserId2",
                    ImageName = "imageUrl",
                    Name = "Some event",
                    Place = "Some place",
                    MaxParticipants = 10,
                    OrganizationId = 2,
                    EventParticipants = new List<EventParticipant> { participant2 },
                    EventOptions = new List<EventOption>()
                }
            };
            _eventsDbSet.SetDbSetData(events.AsQueryable());
            return guids;
        }

        #endregion
    }
}
