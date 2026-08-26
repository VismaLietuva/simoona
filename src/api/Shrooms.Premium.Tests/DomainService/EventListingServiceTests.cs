using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Events.List;
using Shrooms.Tests.Extensions;
using X.PagedList;

namespace Shrooms.Premium.Tests.DomainService
{
    public class EventListingServiceTests
    {
        private DbSet<Event> _eventsDbSet;
        private DbSet<EventType> _eventTypesDbSet;
        private DbSet<Office> _officeDbSet;
        private DbSet<EventParticipant> _eventParticipantsDbSet;
        private DbSet<KudosType> _kudosTypesDbSet;
        private DbSet<KudosLog> _kudosLogDbSet;

        private IEventListingService _eventListingService;
        private ISystemClock _systemClockMock;
        private EventValidationService _eventValidationService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _eventsDbSet = uow.MockDbSetForAsync<Event>();
            _eventTypesDbSet = uow.MockDbSetForAsync<EventType>();
            _officeDbSet = uow.MockDbSetForAsync<Office>();
            _eventParticipantsDbSet = uow.MockDbSetForAsync<EventParticipant>();
            _kudosLogDbSet = uow.MockDbSetForAsync<KudosLog>();
            _kudosTypesDbSet = uow.MockDbSetForAsync<KudosType>();

            _systemClockMock = Substitute.For<ISystemClock>();
            _eventValidationService = new EventValidationService(_systemClockMock);
            var eventValidationService = Substitute.For<IEventValidationService>();

            _eventListingService = new EventListingService(uow, eventValidationService);
        }

        [Test]
        public async Task Should_Return_My_Events_As_A_Participant()
        {
            var eventGuids = MockEventsListTest();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "testUser1"
            };

            var myEventsOptions = new MyEventsOptionsDto
            {
                SearchString = null,
                Filter = MyEventsOptions.Participant,
                Page = 1,
                PageSize = 10
            };

            var result = (await _eventListingService.GetMyEventsAsync(myEventsOptions, userOrg)).ToList();

            ClassicAssert.AreEqual(3, result.Count);
            ClassicAssert.AreEqual(result.First(x => x.Id == eventGuids[0]).ParticipatingStatus, AttendingStatus.Attending);
            ClassicAssert.IsTrue(result.First(x => x.Id == eventGuids[2]).StartDate < result.First(x => x.Id == eventGuids[0]).StartDate);
        }

        [Test]
        public async Task Should_Return_My_Events_As_A_Master()
        {
            var eventGuids = MockEventsListTest();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "responsibleUserId4"
            };

            var myEventsOptions = new MyEventsOptionsDto
            {
                SearchString = null,
                Filter = MyEventsOptions.Host,
                Page = 1,
                PageSize = 10
            };

            var result = (await _eventListingService.GetMyEventsAsync(myEventsOptions, userOrg)).ToList();

            ClassicAssert.AreEqual(1, result.Count);
            ClassicAssert.IsTrue(result.First(x => x.Id == eventGuids[3]).IsCreator);
        }

        [Test]
        public async Task Should_Return_Options_By_Event_Id()
        {
            var eventsGuids = MockEventOptionsWithEvents();
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var result = await _eventListingService.GetEventOptionsAsync(eventsGuids[1], userOrg);

            ClassicAssert.AreEqual(result.Options.Count(), 2);
            ClassicAssert.AreEqual(result.Options.First(o => o.Id == 4).Option, "Option1");
            ClassicAssert.AreEqual(result.Options.First(o => o.Id == 5).Option, "Option2");
        }

        [Test]
        public async Task Should_Keep_Question_Options_Out_Of_The_Legacy_Option_List()
        {
            var eventId = MockEventWithQuestions();
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventListingService.GetEventOptionsAsync(eventId, userOrg);

            Assert.That(result.Options.Select(o => o.Id), Is.EquivalentTo(new[] { 40 }));
        }

        [Test]
        public async Task Should_Return_Questions_And_Their_Options_In_Order()
        {
            var eventId = MockEventWithQuestions();
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventListingService.GetEventOptionsAsync(eventId, userOrg);

            var questions = result.Questions.ToList();
            Assert.That(questions.Select(q => q.Id), Is.EqualTo(new int?[] { 5, 6 }));
            Assert.That(questions[0].Title, Is.EqualTo("Pick your dish"));
            Assert.That(questions[0].SelectType, Is.EqualTo(EventQuestionSelectType.Single));
            Assert.That(questions[0].IsRequired, Is.True);
            Assert.That(questions[0].ShowIfOptionId, Is.Null);
            Assert.That(questions[0].Options.Select(o => o.Id), Is.EqualTo(new int?[] { 50, 51 }));
            Assert.That(questions[0].Options[0].Name, Is.EqualTo("Pasta"));
            Assert.That(questions[1].ShowIfOptionId, Is.EqualTo(51));
        }

        [Test]
        public async Task Should_Return_My_Chosen_Options_For_The_Calling_User()
        {
            var eventId = MockEventWithQuestions();
            _eventParticipantsDbSet.SetDbSetDataForAsync(new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    EventId = eventId,
                    ApplicationUserId = "testUser1",
                    EventOptions = new List<EventOption>
                    {
                        new EventOption { Id = 50 },
                        new EventOption { Id = 40 }
                    }
                },
                new EventParticipant
                {
                    Id = 2,
                    EventId = eventId,
                    ApplicationUserId = "someoneElse",
                    EventOptions = new List<EventOption> { new EventOption { Id = 51 } }
                }
            });
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventListingService.GetEventOptionsAsync(eventId, userOrg);

            Assert.That(result.MyChosenOptions, Is.EquivalentTo(new[] { 40, 50 }));
        }

        [Test]
        public async Task Should_Return_No_Chosen_Options_When_The_Caller_Has_Not_Joined()
        {
            var eventId = MockEventWithQuestions();
            // Someone else's answers, and no row for the caller. Seeding an empty list here
            // would make this test vacuous — MyChosenOptions already defaults to empty, so it
            // would pass with no implementation at all. With another participant present it
            // fails the moment the query stops filtering by user.
            _eventParticipantsDbSet.SetDbSetDataForAsync(new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 2,
                    EventId = eventId,
                    ApplicationUserId = "someoneElse",
                    EventOptions = new List<EventOption> { new EventOption { Id = 51 } }
                }
            });
            var userOrg = new UserAndOrganizationDto { OrganizationId = 2, UserId = "testUser1" };

            var result = await _eventListingService.GetEventOptionsAsync(eventId, userOrg);

            Assert.That(result.MyChosenOptions, Is.Empty);
        }

        [Test]
        public void Should_Throw_If_Event_Deadline_Is_Greater_Than_Start_Date()
        {
            var deadlineDate = DateTime.Parse("2016-05-01");
            var startDate = DateTime.Parse("2016-04-28");
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfRegistrationDeadlineExceedsStartDate(deadlineDate, startDate));
            ClassicAssert.AreEqual(ex.Message, PremiumErrorCodes.EventRegistrationDeadlineGreaterThanStartDateCode);
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
            ClassicAssert.AreEqual(ex.Message, PremiumErrorCodes.EventRegistrationDeadlineIsExpired);
        }

        [Test]
        public void Should_Not_Throw_If_Deadline_Date_Is_Valid()
        {
            var deadlineDate = DateTime.Parse("2016-05-01");
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-05-02"));
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfRegistrationDeadlineIsExpired(deadlineDate));
            ClassicAssert.AreEqual(ex.Message, PremiumErrorCodes.EventRegistrationDeadlineIsExpired);
        }

        [Test]
        public async Task Should_Return_Not_Started_Paged_Events_Filtered_By_Title()
        {
            // Arrange
            MockEventsListTest();

            var reportListingArgs = new EventReportListingArgsDto
            {
                SearchString = "Drinking event",
                EventTypeIds = Enumerable.Empty<int>(),
                OfficeTypeIds = Enumerable.Empty<string>(),
                Page = 1,
                PageSize = 10
            };

            var userOrg = new UserAndOrganizationDto
            {
                UserId = Guid.NewGuid().ToString(),
                OrganizationId = 3
            };

            // Act
            var result = await _eventListingService.GetNotStartedEventsFilteredByTitleAsync(reportListingArgs, userOrg);

            // Assert
            ClassicAssert.IsInstanceOf<IPagedList>(result);
            Assert.That(result, Is.All.Matches<EventDetailsListItemDto>(item => item.Name.Contains(reportListingArgs.SearchString)));
            Assert.That(result, Is.All.Matches<EventDetailsListItemDto>(item => item.StartDate > DateTime.UtcNow));
        }

        [Test]
        public async Task Should_Return_All_Not_Started_Events_If_Search_String_Is_Not_Provided()
        {
            // Arrange
            MockEventsListTest();

            const int expectedCount = 3;

            var reportListingArgs = new EventReportListingArgsDto
            {
                SearchString = null,
                EventTypeIds = Enumerable.Empty<int>(),
                OfficeTypeIds = Enumerable.Empty<string>(),
                Page = 1,
                PageSize = 10
            };

            var userOrg = new UserAndOrganizationDto
            {
                UserId = Guid.NewGuid().ToString(),
                OrganizationId = 2
            };

            // Act
            var result = await _eventListingService.GetNotStartedEventsFilteredByTitleAsync(reportListingArgs, userOrg);

            // Assert
            ClassicAssert.IsInstanceOf<IPagedList>(result);
            ClassicAssert.AreEqual(expectedCount, result.Count);
            Assert.That(result, Is.All.Matches<EventDetailsListItemDto>(item => item.StartDate > DateTime.UtcNow));
        }

        [Test]
        public async Task Should_Return_Not_Started_Paged_Events_Filtered_By_Event_Types()
        {
            // Arrange
            var eventGuids = MockEventsListTest();

            var expectedEventIds = new List<Guid>
            {
                eventGuids[1],
                eventGuids[3]
            };

            var reportListingArgs = new EventReportListingArgsDto
            {
                SearchString = null,
                EventTypeIds = new List<int> { 2 },
                OfficeTypeIds = Enumerable.Empty<string>(),
                Page = 1,
                PageSize = 10
            };

            var userOrg = new UserAndOrganizationDto
            {
                UserId = Guid.NewGuid().ToString(),
                OrganizationId = 3
            };

            // Act
            var result = await _eventListingService.GetNotStartedEventsFilteredByTitleAsync(reportListingArgs, userOrg);

            // Assert
            ClassicAssert.IsInstanceOf<IPagedList>(result);
            Assert.That(result, Is.All.Matches<EventDetailsListItemDto>(item => expectedEventIds.Contains(item.Id)));
            Assert.That(result, Is.All.Matches<EventDetailsListItemDto>(item => item.StartDate > DateTime.UtcNow));
        }

        [Test]
        public async Task Should_Return_Not_Started_Paged_Events_Filtered_By_Office_Types()
        {
            // Arrange
            var eventGuids = MockEventsListTest();

            var expectedEventIds = new List<Guid>
            {
                eventGuids[0],
                eventGuids[2],
                eventGuids[4]
            };

            var reportListingArgs = new EventReportListingArgsDto
            {
                SearchString = null,
                EventTypeIds = Enumerable.Empty<int>(),
                OfficeTypeIds = new List<string> { "1" },
                Page = 1,
                PageSize = 10
            };

            var userOrg = new UserAndOrganizationDto
            {
                UserId = Guid.NewGuid().ToString(),
                OrganizationId = 2
            };

            // Act
            var result = await _eventListingService.GetNotStartedEventsFilteredByTitleAsync(reportListingArgs, userOrg);

            // Assert
            ClassicAssert.IsInstanceOf<IPagedList>(result);
            Assert.That(result, Is.All.Matches<EventDetailsListItemDto>(item => expectedEventIds.Contains(item.Id)));
            Assert.That(result, Is.All.Matches<EventDetailsListItemDto>(item => item.StartDate > DateTime.UtcNow));
        }

        [Test]
        public async Task Should_Return_Not_Started_Paged_Events_Sorted_By_Event_Name()
        {
            // Arrange
            var eventGuids = MockEventsListTest();

            var expectedEventIdsByNameOrder = new List<Guid>
            {
                eventGuids[2],
                eventGuids[0],
                eventGuids[4]
            };

            var reportListingArgs = new EventReportListingArgsDto
            {
                SearchString = null,
                EventTypeIds = Enumerable.Empty<int>(),
                OfficeTypeIds = Enumerable.Empty<string>(),
                Page = 1,
                PageSize = 10,
                SortByProperties = "Name desc"
            };

            var userOrg = new UserAndOrganizationDto
            {
                UserId = Guid.NewGuid().ToString(),
                OrganizationId = 2
            };

            // Act
            var result = await _eventListingService.GetNotStartedEventsFilteredByTitleAsync(reportListingArgs, userOrg);

            // Assert
            ClassicAssert.IsInstanceOf<IPagedList>(result);
            Assert.That(result, Is.All.Matches<EventDetailsListItemDto>(item => item.StartDate > DateTime.UtcNow));
            CollectionAssert.AreEqual(expectedEventIdsByNameOrder, result.Select(item => item.Id));
        }

        [Test]
        public async Task Should_Return_Report_Participants()
        {
            // Arrange
            var eventWithParticipants = MockEventReportParticipantsTest()[0];
            const int expectedParticipantCount = 2;

            var reportListingArgs = new EventParticipantsReportListingArgsDto
            {
                EventId = eventWithParticipants.Id,
                KudosTypeIds = Enumerable.Empty<int>(),
                EventTypeIds = Enumerable.Empty<int>(),
                Page = 1,
                PageSize = 10,
                StartDate = DateTime.UtcNow.AddYears(-1),
                EndDate = DateTime.UtcNow.AddYears(5)
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 3,
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _eventListingService.GetReportParticipantsAsync(reportListingArgs, userOrg);

            // Arrange
            ClassicAssert.AreEqual(expectedParticipantCount, result.Count);
        }

        [Test]
        public async Task Should_Return_Report_Participants_With_Filtered_VisitedEvents()
        {
            // Arrange
            var eventWithParticipants = MockEventReportParticipantsTest()[0];
            const int expectedVisitedEventsCount = 2;

            var reportListingArgs = new EventParticipantsReportListingArgsDto
            {
                EventId = eventWithParticipants.Id,
                KudosTypeIds = Enumerable.Empty<int>(),
                EventTypeIds = new List<int> { 1 },
                Page = 1,
                PageSize = 10,
                StartDate = DateTime.UtcNow.AddYears(-1),
                EndDate = DateTime.UtcNow.AddYears(5)
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 3,
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _eventListingService.GetReportParticipantsAsync(reportListingArgs, userOrg);

            // Arrange
            ClassicAssert.AreEqual(expectedVisitedEventsCount, result.First().VisitedEvents.Count);
            Assert.That(result.First().VisitedEvents, Is.All.Matches<EventVisitedReportDto>(visited => visited.TypeName == "Cool type"));
        }

        [Test]
        public async Task Should_Return_Report_Participants_With_Only_Ended_Visited_Events()
        {
            // Arrange
            var eventWithParticipants = MockEventReportParticipantsTest()[0];

            var reportListingArgs = new EventParticipantsReportListingArgsDto
            {
                EventId = eventWithParticipants.Id,
                KudosTypeIds = Enumerable.Empty<int>(),
                EventTypeIds = Enumerable.Empty<int>(),
                Page = 1,
                PageSize = 10,
                StartDate = DateTime.UtcNow.AddYears(-1),
                EndDate = DateTime.UtcNow.AddYears(5)
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 3,
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _eventListingService.GetReportParticipantsAsync(reportListingArgs, userOrg);

            // Arrange
            Assert.That(result.First().VisitedEvents, Is.All.Matches<EventVisitedReportDto>(visited => visited.EndDate < DateTime.UtcNow));
        }

        [Test]
        public async Task Should_Return_Report_Participants_With_Visited_Events_Ordered_By_Descending_End_Date()
        {
            // Arrange
            var eventWithParticipants = MockEventReportParticipantsTest()[0];

            var expectedOrder = new List<int>
            {
                DateTime.UtcNow.AddDays(-1).DayOfYear,
                DateTime.UtcNow.AddDays(-2).DayOfYear,
                DateTime.UtcNow.AddDays(-3).DayOfYear
            };

            var reportListingArgs = new EventParticipantsReportListingArgsDto
            {
                EventId = eventWithParticipants.Id,
                KudosTypeIds = Enumerable.Empty<int>(),
                EventTypeIds = Enumerable.Empty<int>(),
                Page = 1,
                PageSize = 10,
                StartDate = DateTime.UtcNow.AddYears(-1),
                EndDate = DateTime.UtcNow.AddYears(5)
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 3,
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _eventListingService.GetReportParticipantsAsync(reportListingArgs, userOrg);

            // Arrange
            CollectionAssert.AreEqual(expectedOrder, result.First().VisitedEvents.Select(visited => visited.EndDate.DayOfYear));
        }

        [Test]
        public async Task Should_Return_Paged_Report_Participants_With_Not_Filtered_Kudos_And_Includes_Only_Approved()
        {
            // Arrange
            var eventWithParticipants = MockEventReportParticipantsTest()[0];
            const int expectedKudosPoints = 30;
            var reportListingArgs = new EventParticipantsReportListingArgsDto
            {
                EventId = eventWithParticipants.Id,
                KudosTypeIds = Enumerable.Empty<int>(),
                EventTypeIds = Enumerable.Empty<int>(),
                SortByProperties = "Kudos desc",
                Page = 1,
                PageSize = 10,
                StartDate = DateTime.UtcNow.AddYears(-1),
                EndDate = DateTime.UtcNow.AddYears(5)
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 3,
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _eventListingService.GetReportParticipantsAsync(reportListingArgs, userOrg);

            // Arrange
            ClassicAssert.AreEqual(expectedKudosPoints, result.First().Kudos);
        }

        [Test]
        public async Task Should_Return_Report_Participants_With_Filtered_Kudos_And_Includes_Only_Approved()
        {
            // Arrange
            var eventWithParticipants = MockEventReportParticipantsTest()[0];
            const int expectedKudosPoints = 20;
            var reportListingArgs = new EventParticipantsReportListingArgsDto
            {
                EventId = eventWithParticipants.Id,
                KudosTypeIds = new List<int> { 1 },
                EventTypeIds = Enumerable.Empty<int>(),
                SortByProperties = "Kudos desc",
                Page = 1,
                PageSize = 10,
                StartDate = DateTime.UtcNow.AddYears(-1),
                EndDate = DateTime.UtcNow.AddYears(5)
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 3,
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _eventListingService.GetReportParticipantsAsync(reportListingArgs, userOrg);

            // Arrange
            ClassicAssert.AreEqual(expectedKudosPoints, result.First().Kudos);
        }

        [Test]
        public async Task Should_Return_Visited_Events_Filtered_By_Event_Type()
        {
            // Arrange
            var expectedCount = 1;

            MockVisitedReportEvents();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var args = new EventParticipantVisitedEventsListingArgsDto
            {
                UserId = "testUser1",
                Page = 1,
                PageSize = 10,
                EventTypeIds = new List<int> { 1 },
                EndDate = DateTime.UtcNow.AddYears(10),
                StartDate = DateTime.UtcNow.AddYears(-10)
            };

            // Act
            var result = await _eventListingService.GetEventParticipantVisitedReportEventsAsync(args, userOrg);

            // Assert
            ClassicAssert.AreEqual(expectedCount, result.Count);
        }

        [Test]
        public async Task Should_Return_Preview_Visited_Events_Filtered_By_Date_Interval()
        {
            // Arrange
            var @event = MockEventReportParticipantsTest()[0];
            var startDate = DateTime.UtcNow.AddDays(-20);
            var endDate = DateTime.UtcNow;

            var reportListingArgs = new EventParticipantsReportListingArgsDto
            {
                EventId = @event.Id,
                KudosTypeIds = Enumerable.Empty<int>(),
                EventTypeIds = Enumerable.Empty<int>(),
                Page = 1,
                PageSize = 10,
                StartDate = startDate,
                EndDate = endDate
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 3,
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _eventListingService.GetReportParticipantsAsync(reportListingArgs, userOrg);

            // Assert
            Assert.That(result.SelectMany(participant => participant.VisitedEvents),
                Is.All.Matches<EventVisitedReportDto>(visitedEvent => visitedEvent.StartDate >= startDate && visitedEvent.EndDate <= endDate));
        }

        [Test]
        public async Task Should_Return_Visited_Events_Filtered_By_Date_Interval()
        {
            // Arrange
            MockVisitedReportEvents();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var args = new EventParticipantVisitedEventsListingArgsDto
            {
                UserId = "testUser1",
                Page = 1,
                PageSize = 10,
                EventTypeIds = new List<int> { 1 },
                EndDate = DateTime.UtcNow.AddYears(1),
                StartDate = DateTime.UtcNow.AddDays(-5)
            };

            // Act
            var result = await _eventListingService.GetEventParticipantVisitedReportEventsAsync(args, userOrg);

            // Assert
            Assert.That(result, Is.All.Matches<EventDetailsListItemDto>(item => item.StartDate >= args.StartDate && item.EndDate <= args.EndDate));
        }

        [Test]
        public async Task Should_Return_Visited_Events_Ordered_By_Descending_End_Date()
        {
            // Arrange
            var guids = MockVisitedReportEvents();

            var expectedEventIdsByNameOrder = new List<Guid>
            {
                guids[2],
                guids[3],
                guids[0]
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var args = new EventParticipantVisitedEventsListingArgsDto
            {
                UserId = "testUser1",
                Page = 1,
                PageSize = 10,
                EventTypeIds = new List<int>(),
                EndDate = DateTime.UtcNow.AddYears(10),
                StartDate = DateTime.UtcNow.AddYears(-10),
                SortByProperties = "Name desc;"
            };

            // Act
            var result = await _eventListingService.GetEventParticipantVisitedReportEventsAsync(args, userOrg);

            // Assert
            CollectionAssert.AreEqual(expectedEventIdsByNameOrder, result.Select(item => item.Id));
        }

        [Test]
        public async Task Should_Return_No_Food_Team_When_Organization_Has_No_Food_Event_Type()
        {
            // Arrange
            MockFoodTeamEvents();

            // Act
            var result = await _eventListingService.GetMyFoodTeamAsync(new UserAndOrganizationDto
            {
                OrganizationId = 3,
                UserId = "testUser1"
            });

            // Assert
            ClassicAssert.IsNull(result.EventTypeId);
            ClassicAssert.IsNull(result.JoinedEvent);
        }

        [Test]
        public async Task Should_Return_Food_Event_Type_Without_Joined_Event_When_User_Has_Not_Joined()
        {
            // Arrange
            MockFoodTeamEvents();

            // Act
            var result = await _eventListingService.GetMyFoodTeamAsync(new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "userWithoutFoodTeam"
            });

            // Assert
            ClassicAssert.AreEqual(10, result.EventTypeId);
            ClassicAssert.IsNull(result.JoinedEvent);
        }

        [Test]
        public async Task Should_Return_Joined_Food_Team_Within_The_Eight_Day_Horizon()
        {
            // Arrange
            var guids = MockFoodTeamEvents();

            // Act
            var result = await _eventListingService.GetMyFoodTeamAsync(new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "testUser1"
            });

            // Assert
            ClassicAssert.AreEqual(10, result.EventTypeId);
            ClassicAssert.AreEqual(guids[0], result.JoinedEvent.Id);
            ClassicAssert.AreEqual("Pizza Friday", result.JoinedEvent.Name);
            ClassicAssert.AreEqual("Kitchen", result.JoinedEvent.Place);
        }

        // This week's food day is over, so the widget rolls over to next week's team even though
        // it starts a little more than seven days out.
        [Test]
        public async Task Should_Return_Next_Weeks_Food_Team_When_This_Weeks_Is_Over()
        {
            // Arrange
            var guids = MockFoodTeamEvents();

            // Act
            var result = await _eventListingService.GetMyFoodTeamAsync(new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "testUserJoinedNextWeek"
            });

            // Assert
            ClassicAssert.AreEqual(guids[5], result.JoinedEvent.Id);
            ClassicAssert.AreEqual("Next week pizza", result.JoinedEvent.Name);
        }

        [Test]
        public async Task Should_Not_Return_Food_Team_Joined_Beyond_The_Coming_Food_Day()
        {
            // Arrange
            MockFoodTeamEvents();

            // Act
            var result = await _eventListingService.GetMyFoodTeamAsync(new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "testUserJoinedFarFuture"
            });

            // Assert
            ClassicAssert.IsNull(result.JoinedEvent);
        }

        [Test]
        public async Task Should_Return_The_Later_Food_Team_When_An_Earlier_One_Already_Finished_Today()
        {
            // Arrange
            var guids = MockFoodTeamEvents();

            // Act
            var result = await _eventListingService.GetMyFoodTeamAsync(new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "testUserTwoTeamsSameDay"
            });

            // Assert
            ClassicAssert.AreEqual(guids[4], result.JoinedEvent.Id);
            ClassicAssert.AreEqual("Late lunch pizza", result.JoinedEvent.Name);
        }

        [Test]
        public async Task Should_Not_Return_Food_Team_The_User_Declined()
        {
            // Arrange
            MockFoodTeamEvents();

            // Act
            var result = await _eventListingService.GetMyFoodTeamAsync(new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "testUserNotAttending"
            });

            // Assert
            ClassicAssert.IsNull(result.JoinedEvent);
        }

        [Test]
        public async Task Should_Not_Return_Food_Team_Of_A_Non_Food_Event_Type()
        {
            // Arrange
            MockFoodTeamEvents();

            // Act
            var result = await _eventListingService.GetMyFoodTeamAsync(new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "testUserJoinedOtherType"
            });

            // Assert
            ClassicAssert.IsNull(result.JoinedEvent);
        }

        [Test]
        public async Task Should_Return_The_Type_Of_The_Joined_Food_Team_When_Organization_Has_Several()
        {
            // Arrange
            var guids = MockFoodTeamEvents();

            // Act
            var result = await _eventListingService.GetMyFoodTeamAsync(new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "testUserJoinedSecondFoodType"
            });

            // Assert
            ClassicAssert.AreEqual(13, result.EventTypeId);
            ClassicAssert.AreEqual(guids[6], result.JoinedEvent.Id);
        }

        #region Mocks

        private Guid[] MockFoodTeamEvents()
        {
            var guids = Enumerable.Repeat(0, 7).Select(_ => Guid.NewGuid()).ToArray();
            var now = DateTime.UtcNow;

            var eventTypes = new List<EventType>
            {
                new EventType
                {
                    Id = 10,
                    Name = "Food",
                    OrganizationId = 2,
                    IsSingleJoin = true,
                    SendWeeklyReminders = true
                },
                new EventType
                {
                    Id = 11,
                    Name = "Sports",
                    OrganizationId = 2,
                    IsSingleJoin = true,
                    SendWeeklyReminders = false
                },
                new EventType
                {
                    Id = 12,
                    Name = "Food",
                    OrganizationId = 3,
                    IsSingleJoin = false,
                    SendWeeklyReminders = true
                },

                // A second food type in the same organization, so the widget has to pick one.
                new EventType
                {
                    Id = 13,
                    Name = "Breakfast",
                    OrganizationId = 2,
                    IsSingleJoin = true,
                    SendWeeklyReminders = true
                }
            };

            var events = new List<Event>
            {
                // Ongoing, so it always falls inside the current week regardless of when the test runs.
                new Event
                {
                    Id = guids[0],
                    Name = "Pizza Friday",
                    Place = "Kitchen",
                    ImageName = "pizza.png",
                    StartDate = now.AddHours(-1),
                    EndDate = now.AddHours(1),
                    OrganizationId = 2,
                    EventTypeId = 10,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant { Id = 1, ApplicationUserId = "testUser1", AttendStatus = (int)AttendingStatus.Attending },
                        new EventParticipant { Id = 2, ApplicationUserId = "testUserNotAttending", AttendStatus = (int)AttendingStatus.NotAttending }
                    }
                },
                new Event
                {
                    Id = guids[1],
                    Name = "Far future pizza",
                    Place = "Kitchen",
                    StartDate = now.AddDays(10),
                    EndDate = now.AddDays(10).AddHours(2),
                    OrganizationId = 2,
                    EventTypeId = 10,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant { Id = 3, ApplicationUserId = "testUserJoinedFarFuture", AttendStatus = (int)AttendingStatus.Attending }
                    }
                },

                // Next week's team, scheduled slightly later in the day than this week's.
                new Event
                {
                    Id = guids[5],
                    Name = "Next week pizza",
                    Place = "Kitchen",
                    StartDate = now.AddDays(7).AddHours(2),
                    EndDate = now.AddDays(7).AddHours(3),
                    OrganizationId = 2,
                    EventTypeId = 10,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant { Id = 7, ApplicationUserId = "testUserJoinedNextWeek", AttendStatus = (int)AttendingStatus.Attending }
                    }
                },
                new Event
                {
                    Id = guids[2],
                    Name = "Basketball",
                    Place = "Court",
                    StartDate = now.AddHours(-1),
                    EndDate = now.AddHours(1),
                    OrganizationId = 2,
                    EventTypeId = 11,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant { Id = 4, ApplicationUserId = "testUserJoinedOtherType", AttendStatus = (int)AttendingStatus.Attending }
                    }
                },

                // Two food teams the same day for one user: the first has already finished.
                new Event
                {
                    Id = guids[3],
                    Name = "Early lunch pizza",
                    Place = "Kitchen",
                    StartDate = now.AddHours(-2),
                    EndDate = now.AddMinutes(-30),
                    OrganizationId = 2,
                    EventTypeId = 10,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant { Id = 5, ApplicationUserId = "testUserTwoTeamsSameDay", AttendStatus = (int)AttendingStatus.Attending }
                    }
                },
                new Event
                {
                    Id = guids[4],
                    Name = "Late lunch pizza",
                    Place = "Kitchen",
                    StartDate = now.AddMinutes(30),
                    EndDate = now.AddHours(2),
                    OrganizationId = 2,
                    EventTypeId = 10,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant { Id = 6, ApplicationUserId = "testUserTwoTeamsSameDay", AttendStatus = (int)AttendingStatus.Attending }
                    }
                },
                new Event
                {
                    Id = guids[6],
                    Name = "Breakfast club",
                    Place = "Kitchen",
                    StartDate = now.AddHours(-1),
                    EndDate = now.AddHours(1),
                    OrganizationId = 2,
                    EventTypeId = 13,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant { Id = 8, ApplicationUserId = "testUserJoinedSecondFoodType", AttendStatus = (int)AttendingStatus.Attending }
                    }
                }
            };

            _eventTypesDbSet.SetDbSetDataForAsync(eventTypes);
            _eventsDbSet.SetDbSetDataForAsync(events);

            return guids;
        }

        private Guid[] MockVisitedReportEvents()
        {
            var guids = Enumerable.Repeat(0, 9).Select(_ => Guid.NewGuid()).ToArray();

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

            var participant4 = new EventParticipant
            {
                ApplicationUserId = "responsibleUserId4",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 4,
                EventId = guids[4],
                AttendStatus = 1,
                Event = new Event(),
                ApplicationUser = new ApplicationUser
                {
                    OrganizationId = 2,
                    Id = "responsibleUserId4",
                    FirstName = "Name",
                    LastName = "Baraton",
                    EmploymentDate = DateTime.UtcNow,
                    QualificationLevel = new QualificationLevel
                    {
                        Name = "Epic qualification"
                    },
                    JobPosition = new JobPosition
                    {
                        Title = ".NET Developer"
                    },
                    Manager = new ApplicationUser
                    {
                        Id = "PiotrPetrovic",
                        FirstName = "Piotr",
                        LastName = "Petrovic"
                    },
                    Projects = new List<Project>
                    {
                        new Project
                        {
                            Id = 1,
                            Name = "Cool project"
                        }
                    },
                    Events = new List<Event>()
                }
            };

            var participant5 = new EventParticipant
            {
                ApplicationUserId = "responsibleUserId5",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 5,
                EventId = guids[4],
                AttendStatus = 1,
                Event = new Event(),
                ApplicationUser = new ApplicationUser
                {
                    OrganizationId = 2,
                    Id = "testUser1",
                    FirstName = "Cool name",
                    LastName = "Baraton",
                    EmploymentDate = DateTime.UtcNow,
                    QualificationLevel = new QualificationLevel
                    {
                        Name = "Epic qualification"
                    },
                    JobPosition = new JobPosition
                    {
                        Title = ".NET Developer"
                    },
                    Manager = new ApplicationUser
                    {
                        Id = "PiotrPetrovic",
                        FirstName = "Piotr",
                        LastName = "Petrovic"
                    },
                    Projects = new List<Project>
                    {
                        new Project
                        {
                            Id = 1,
                            Name = "Cool project"
                        }
                    },
                    Events = new List<Event>()
                }
            };

            var participant6 = new EventParticipant
            {
                ApplicationUserId = "responsibleUserId6",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 6,
                EventId = guids[5],
                AttendStatus = 1,
                Event = new Event(),
                ApplicationUser = new ApplicationUser
                {
                    OrganizationId = 3,
                    Id = "responsibleUserId6",
                    FirstName = "Anton",
                    LastName = "Baraton",
                    EmploymentDate = DateTime.UtcNow,
                    QualificationLevel = new QualificationLevel
                    {
                        Name = "Epic qualification"
                    },
                    JobPosition = new JobPosition
                    {
                        Title = ".NET Developer"
                    },
                    Manager = new ApplicationUser
                    {
                        Id = "PiotrPetrovic",
                        FirstName = "Piotr",
                        LastName = "Petrovic"
                    },
                    Projects = new List<Project>
                    {
                        new Project
                        {
                            Id = 1,
                            Name = "Cool project"
                        }
                    },
                    Events = new List<Event>
                    {
                        new Event
                        {
                            EndDate = DateTime.UtcNow.AddDays(-1),
                            StartDate = DateTime.UtcNow.AddDays(-2),
                            EventType = new EventType
                            {
                                Id = 1,
                                Name = "Cool type"
                            },
                            OrganizationId = 3,
                            Name = "Some kind of event"
                        },

                        new Event
                        {
                            EndDate = DateTime.UtcNow.AddDays(-2),
                            StartDate = DateTime.UtcNow.AddDays(-3),
                            EventType = new EventType
                            {
                                Id = 2,
                                Name = "Awesome type"
                            },
                            OrganizationId = 3,
                            Name = "Racing"
                        },

                        new Event
                        {
                            EndDate = DateTime.UtcNow.AddDays(-3),
                            StartDate = DateTime.UtcNow.AddDays(-4),
                            EventType = new EventType
                            {
                                Id = 1,
                                Name = "Cool type"
                            },
                            OrganizationId = 3,
                            Name = "Running"
                        }
                    }
                }
            };

            var events = new List<Event>
            {
                new Event
                {
                    Id = guids[0],
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddDays(-5),
                    Created = DateTime.UtcNow,
                    EventTypeId = 1,
                    ResponsibleUserId = "responsibleUserId",
                    ImageName = "imageUrl",
                    Name = "Drinking event",
                    Place = "City",
                    MaxParticipants = 15,
                    OrganizationId = 2,
                    EventParticipants = new List<EventParticipant> { participant1, participant2 },
                    EventOptions = new List<EventOption>(),
                    EventType = new EventType
                    {
                        Id = 1,
                        IsShownWithMainEvents = true
                    },
                    Offices = "[\"1\", \"2\"]"
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
                    EventOptions = new List<EventOption>(),
                    EventType = new EventType
                    {
                        Id = 2,
                        IsShownWithMainEvents = true
                    },
                    Offices = "[\"1\", \"2\"]"
                },
                new Event
                {
                    Id = guids[2],
                    StartDate = DateTime.UtcNow.AddDays(-6),
                    EndDate = DateTime.UtcNow.AddYears(-5),
                    Created = DateTime.UtcNow,
                    EventTypeId = 3,
                    ResponsibleUserId = "responsibleUserId",
                    ImageName = "imageUrl",
                    Name = "Some event",
                    Place = "Some place",
                    MaxParticipants = 10,
                    OrganizationId = 2,
                    EventParticipants = new List<EventParticipant> { participant2 },
                    EventOptions = new List<EventOption>(),
                    EventType = new EventType
                    {
                        Id = 3,
                        IsShownWithMainEvents = true
                    },
                    Offices = "[\"1\", \"2\"]"
                },
                new Event
                {
                    Id = guids[3],
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddDays(-5),
                    Created = DateTime.UtcNow,
                    EventTypeId = 3,
                    ResponsibleUserId = "responsibleUserId2",
                    ImageName = "imageUrl",
                    Name = "Some event",
                    Place = "Some place",
                    MaxParticipants = 10,
                    OrganizationId = 2,
                    EventParticipants = new List<EventParticipant> { participant2 },
                    EventOptions = new List<EventOption>(),
                    EventType = new EventType
                    {
                        Id = 3,
                        IsShownWithMainEvents = true
                    },
                    Offices = "[\"1\"]"
                },
                new Event
                {
                    Id = guids[4],
                    StartDate = DateTime.UtcNow.AddYears(-5),
                    EndDate = DateTime.UtcNow.AddYears(-1),
                    Created = DateTime.UtcNow,
                    EventTypeId = 3,
                    ResponsibleUserId = "responsibleUserId3",
                    ImageName = "imageUrl",
                    Name = "Cool event",
                    Place = "Cool place",
                    MaxParticipants = 10,
                    OrganizationId = 2,
                    EventParticipants = new List<EventParticipant> { participant4, participant5 },
                    EventOptions = new List<EventOption>(),
                    EventType = new EventType
                    {
                        Id = 3,
                        IsShownWithMainEvents = true
                    },
                    Offices = "[\"1\"]"
                },
                new Event
                {
                    Id = guids[5],
                    StartDate = DateTime.UtcNow.AddDays(10),
                    EndDate = DateTime.UtcNow.AddDays(21),
                    Created = DateTime.UtcNow,
                    EventTypeId = 3,
                    ResponsibleUserId = "responsibleUserId3",
                    ImageName = "imageUrl",
                    Name = "Cool event",
                    Place = "Cool place",
                    MaxParticipants = 10,
                    OrganizationId = 3,
                    EventParticipants = new List<EventParticipant> { participant6 },
                    EventOptions = new List<EventOption>(),
                    EventType = new EventType
                    {
                        Id = 3,
                        IsShownWithMainEvents = true
                    },
                    Offices = "[\"2\"]"
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(events);
            _eventParticipantsDbSet.SetDbSetDataForAsync(new List<EventParticipant>
            {
                participant1,
                participant2,
                participant3,
                participant4,
                participant5,
                participant6
            });

            return guids;
        }

        private Guid[] MockEventOptionsWithEvents()
        {
            var guids = Enumerable.Repeat(0, 2).Select(_ => Guid.NewGuid()).ToArray();

            var options1 = new List<EventOption>
            {
                new EventOption
                {
                    Id = 1,
                    EventId = guids[0],
                    Option = "Option1"
                },
                new EventOption
                {
                    Id = 2,
                    EventId = guids[0],
                    Option = "Option2"
                },
                new EventOption
                {
                    Id = 3,
                    EventId = guids[0],
                    Option = "Option3"
                }
            };

            var options2 = new List<EventOption>
            {
                new EventOption
                {
                    Id = 4,
                    EventId = guids[1],
                    Option = "Option1"
                },
                new EventOption
                {
                    Id = 5,
                    EventId = guids[1],
                    Option = "Option2"
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
                    EventOptions = options1,
                    EventQuestions = new List<EventQuestion>()
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
                    EventOptions = options2,
                    EventQuestions = new List<EventQuestion>()
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());

            return guids;
        }

        private Guid MockEventWithQuestions()
        {
            var eventId = Guid.NewGuid();

            var legacyOption = new EventOption { Id = 40, EventId = eventId, Option = "Vegetarian", QuestionId = null };

            var dishOptions = new List<EventOption>
            {
                new EventOption { Id = 51, EventId = eventId, Option = "Pizza", QuestionId = 5, Order = 1 },
                new EventOption { Id = 50, EventId = eventId, Option = "Pasta", QuestionId = 5, Order = 0 }
            };

            var questions = new List<EventQuestion>
            {
                new EventQuestion
                {
                    Id = 6,
                    EventId = eventId,
                    Title = "Which pizza?",
                    Order = 1,
                    SelectType = EventQuestionSelectType.Single,
                    IsRequired = false,
                    ShowIfOptionId = 51,
                    Options = new List<EventOption>()
                },
                new EventQuestion
                {
                    Id = 5,
                    EventId = eventId,
                    Title = "Pick your dish",
                    Order = 0,
                    SelectType = EventQuestionSelectType.Single,
                    IsRequired = true,
                    ShowIfOptionId = null,
                    Options = dishOptions
                }
            };

            var events = new List<Event>
            {
                new Event
                {
                    Id = eventId,
                    MaxChoices = 0,
                    MaxParticipants = 20,
                    OrganizationId = 2,
                    Name = "Question event",
                    EventTypeId = 1,
                    EventType = new EventType { Id = 1, Name = "test type", IsSingleJoin = false },
                    EventParticipants = new List<EventParticipant>(),
                    EventOptions = new List<EventOption> { legacyOption, dishOptions[0], dishOptions[1] },
                    EventQuestions = questions
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());

            return eventId;
        }

        private IList<Event> MockEventReportParticipantsTest()
        {
            var events = new List<Event>
            {
                new Event
                {
                    Id = Guid.NewGuid(),
                    EndDate = DateTime.UtcNow.AddDays(-1),
                    StartDate = DateTime.UtcNow.AddDays(-2),
                    EventType = new EventType
                    {
                        Id = 1,
                        Name = "Cool type"
                    },
                    OrganizationId = 3,
                    Name = "Some kind of event",
                    EventParticipants = new List<EventParticipant>()
                },

                new Event
                {
                    Id = Guid.NewGuid(),
                    EndDate = DateTime.UtcNow.AddDays(-2),
                    StartDate = DateTime.UtcNow.AddDays(-3),
                    EventType = new EventType
                    {
                        Id = 2,
                        Name = "Awesome type"
                    },
                    OrganizationId = 3,
                    Name = "Racing",
                    EventParticipants = new List<EventParticipant>()
                },

                new Event
                {
                    Id = Guid.NewGuid(),
                    EndDate = DateTime.UtcNow.AddDays(-3),
                    StartDate = DateTime.UtcNow.AddDays(-4),
                    EventType = new EventType
                    {
                        Id = 1,
                        Name = "Cool type"
                    },
                    OrganizationId = 3,
                    Name = "Running",
                    EventParticipants = new List<EventParticipant>()
                }
            };

            var participantUserIds = Enumerable.Repeat(0, 2).Select(_ => Guid.NewGuid().ToString()).ToList();

            var kudosLogs = new List<KudosLog>
            {
                new KudosLog
                {
                    EmployeeId = participantUserIds[0],
                    Status = KudosStatus.Approved,
                    OrganizationId = 3,
                    Points = 10,
                    KudosTypeName = "Some name"
                },

                new KudosLog
                {
                    EmployeeId = participantUserIds[0],
                    Status = KudosStatus.Approved,
                    OrganizationId = 3,
                    Points = 10,
                    KudosTypeName = "Some name"
                },

                new KudosLog
                {
                    EmployeeId = participantUserIds[0],
                    Status = KudosStatus.Pending,
                    OrganizationId = 3,
                    Points = 10,
                    KudosTypeName = "Some name"
                },

                new KudosLog
                {
                    EmployeeId = participantUserIds[0],
                    Status = KudosStatus.Rejected,
                    OrganizationId = 3,
                    Points = 10,
                    KudosTypeName = "Some name"
                },

                new KudosLog
                {
                    EmployeeId = participantUserIds[0],
                    Status = KudosStatus.Approved,
                    OrganizationId = 3,
                    Points = 10,
                    KudosTypeName = "Awesome type over here"
                }
            };

            var offices = new List<Office>
            {
                new Office
                {
                    Id = 1,
                    Name = "First office"
                },

                new Office
                {
                    Id = 2,
                    Name = "Second office"
                }
            };

            var kudosTypes = new List<KudosType>
            {
                new KudosType
                {
                    Id = 1,
                    Name = "Some name"
                },

                new KudosType
                {
                    Id = 2,
                    Name = "Awesome type over here"
                }
            };

            var managerId = Guid.NewGuid().ToString();

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    EventId = events[0].Id,
                    ApplicationUserId = participantUserIds[0],
                    ApplicationUser = new ApplicationUser
                    {
                        Id = participantUserIds[0],
                        FirstName = "Arnold",
                        LastName = "Something",
                        EmploymentDate = DateTime.UtcNow,
                        OrganizationId = 3,
                        QualificationLevel = new QualificationLevel
                        {
                            Name = "Junior"
                        },
                        JobPosition = new JobPosition
                        {
                            Title = ".NET Developer"
                        },
                        Manager = new ApplicationUser
                        {
                            Id = managerId,
                            FirstName = "Jonas",
                            LastName = "Ma"
                        },
                        Projects = new List<Project>
                        {
                            new Project
                            {
                                Id = 1,
                                Name = "Cool project"
                            }
                        }
                    },
                    AttendStatus = (int)AttendingStatus.AttendingVirtually
                },
                new EventParticipant
                {
                    EventId = events[1].Id,
                    ApplicationUserId = participantUserIds[0],
                    ApplicationUser = new ApplicationUser
                    {
                        Id = participantUserIds[0],
                        FirstName = "Arnold",
                        LastName = "Something",
                        EmploymentDate = DateTime.UtcNow,
                        OrganizationId = 3,
                        QualificationLevel = new QualificationLevel
                        {
                            Name = "Junior"
                        },
                        JobPosition = new JobPosition
                        {
                            Title = ".NET Developer"
                        },
                        Manager = new ApplicationUser
                        {
                            Id = managerId,
                            FirstName = "Jonas",
                            LastName = "Ma"
                        },
                        Projects = new List<Project>
                        {
                            new Project
                            {
                                Id = 1,
                                Name = "Cool project"
                            }
                        }
                    },
                    AttendStatus = (int)AttendingStatus.Attending
                },
                new EventParticipant
                {
                    EventId = events[0].Id,
                    ApplicationUserId = participantUserIds[1],
                    ApplicationUser = new ApplicationUser
                    {
                        Id = participantUserIds[1],
                        OrganizationId = 3,
                        FirstName = "Joker",
                        LastName = "Something",
                        EmploymentDate = DateTime.UtcNow,
                        QualificationLevel = new QualificationLevel
                        {
                            Name = "Junior"
                        },
                        JobPosition = new JobPosition
                        {
                            Title = ".NET Developer"
                        },
                        Manager = new ApplicationUser
                        {
                            Id = managerId,
                            FirstName = "Jonas",
                            LastName = "Ma"
                        },
                        Projects = new List<Project>
                        {
                            new Project
                            {
                                Id = 1,
                                Name = "Cool project"
                            }
                        }
                    },
                    AttendStatus = (int)AttendingStatus.AttendingVirtually
                }
            };

            events[0].EventParticipants = participants;
            events[1].EventParticipants = participants;
            events[2].EventParticipants = participants;

            _eventsDbSet.SetDbSetDataForAsync(events);
            _officeDbSet.SetDbSetDataForAsync(offices);
            _kudosLogDbSet.SetDbSetDataForAsync(kudosLogs);
            _kudosTypesDbSet.SetDbSetDataForAsync(kudosTypes);
            _eventParticipantsDbSet.SetDbSetDataForAsync(participants);

            return events;
        }

        private Guid[] MockEventsListTest()
        {
            var guids = Enumerable.Repeat(0, 9).Select(_ => Guid.NewGuid()).ToArray();

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

            var participant4 = new EventParticipant
            {
                ApplicationUserId = "responsibleUserId4",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 4,
                EventId = guids[4],
                AttendStatus = 1,
                Event = new Event(),
                ApplicationUser = new ApplicationUser
                {
                    OrganizationId = 2,
                    Id = "testUser2",
                    FirstName = "Name",
                    LastName = "Baraton",
                    EmploymentDate = DateTime.UtcNow,
                    QualificationLevel = new QualificationLevel
                    {
                        Name = "Epic qualification"
                    },
                    JobPosition = new JobPosition
                    {
                        Title = ".NET Developer"
                    },
                    Manager = new ApplicationUser
                    {
                        Id = "PiotrPetrovic",
                        FirstName = "Piotr",
                        LastName = "Petrovic"
                    },
                    Projects = new List<Project>
                    {
                        new Project
                        {
                            Id = 1,
                            Name = "Cool project"
                        }
                    },
                    Events = new List<Event>()
                }
            };

            var participant5 = new EventParticipant
            {
                ApplicationUserId = "responsibleUserId5",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 5,
                EventId = guids[4],
                AttendStatus = 1,
                Event = new Event(),
                ApplicationUser = new ApplicationUser
                {
                    OrganizationId = 2,
                    Id = "testUser1",
                    FirstName = "Cool name",
                    LastName = "Baraton",
                    EmploymentDate = DateTime.UtcNow,
                    QualificationLevel = new QualificationLevel
                    {
                        Name = "Epic qualification"
                    },
                    JobPosition = new JobPosition
                    {
                        Title = ".NET Developer"
                    },
                    Manager = new ApplicationUser
                    {
                        Id = "PiotrPetrovic",
                        FirstName = "Piotr",
                        LastName = "Petrovic"
                    },
                    Projects = new List<Project>
                    {
                        new Project
                        {
                            Id = 1,
                            Name = "Cool project"
                        }
                    },
                    Events = new List<Event>()
                }
            };

            var participant6 = new EventParticipant
            {
                ApplicationUserId = "responsibleUserId6",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 6,
                EventId = guids[5],
                AttendStatus = 1,
                Event = new Event(),
                ApplicationUser = new ApplicationUser
                {
                    OrganizationId = 3,
                    Id = "responsibleUserId6",
                    FirstName = "Anton",
                    LastName = "Baraton",
                    EmploymentDate = DateTime.UtcNow,
                    QualificationLevel = new QualificationLevel
                    {
                        Name = "Epic qualification"
                    },
                    JobPosition = new JobPosition
                    {
                        Title = ".NET Developer"
                    },
                    Manager = new ApplicationUser
                    {
                        Id = "PiotrPetrovic",
                        FirstName = "Piotr",
                        LastName = "Petrovic"
                    },
                    Projects = new List<Project>
                    {
                        new Project
                        {
                            Id = 1,
                            Name = "Cool project"
                        }
                    },
                    Events = new List<Event>
                    {
                        new Event
                        {
                            EndDate = DateTime.UtcNow.AddDays(-1),
                            StartDate = DateTime.UtcNow.AddDays(-2),
                            EventType = new EventType
                            {
                                Id = 1,
                                Name = "Cool type"
                            },
                            OrganizationId = 3,
                            Name = "Some kind of event"
                        },

                        new Event
                        {
                            EndDate = DateTime.UtcNow.AddDays(-2),
                            StartDate = DateTime.UtcNow.AddDays(-3),
                            EventType = new EventType
                            {
                                Id = 2,
                                Name = "Awesome type"
                            },
                            OrganizationId = 3,
                            Name = "Racing"
                        },

                        new Event
                        {
                            EndDate = DateTime.UtcNow.AddDays(-3),
                            StartDate = DateTime.UtcNow.AddDays(-4),
                            EventType = new EventType
                            {
                                Id = 1,
                                Name = "Cool type"
                            },
                            OrganizationId = 3,
                            Name = "Running"
                        }
                    }
                }
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
                    ResponsibleUserId = "responsibleUserId1",
                    ResponsibleUser = new ApplicationUser
                    {
                        Id = "responsibleUserId1",
                        FirstName = "Responsible1",
                        LastName = "User1"
                    },
                    ImageName = "imageUrl",
                    Name = "Drinking event",
                    Place = "City",
                    MaxParticipants = 15,
                    OrganizationId = 2,
                    EventParticipants = new List<EventParticipant> { participant1, participant2 },
                    EventOptions = new List<EventOption>(),
                    EventType = new EventType
                    {
                        Id = 1,
                        IsShownWithMainEvents = true
                    },
                    Offices = "[\"1\", \"2\"]"
                },
                new Event
                {
                    Id = guids[1],
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    Created = DateTime.UtcNow,
                    EventTypeId = 2,
                    ResponsibleUserId = "responsibleUserId2",
                    ResponsibleUser = new ApplicationUser
                    {
                        Id = "responsibleUserId2",
                        FirstName = "Responsible2",
                        LastName = "User2"
                    },
                    ImageName = "imageUrl",
                    Name = "Drinking event",
                    Place = "City",
                    MaxParticipants = 15,
                    OrganizationId = 3,
                    EventParticipants = new List<EventParticipant> { participant3 },
                    EventOptions = new List<EventOption>(),
                    EventType = new EventType
                    {
                        Id = 2,
                        IsShownWithMainEvents = true
                    },
                    Offices = "[\"1\", \"2\"]"
                },
                new Event
                {
                    Id = guids[2],
                    StartDate = DateTime.UtcNow.AddDays(3),
                    EndDate = DateTime.UtcNow.AddDays(3),
                    Created = DateTime.UtcNow,
                    EventTypeId = 3,
                    ResponsibleUserId = "responsibleUserId3",
                    ResponsibleUser = new ApplicationUser
                    {
                        Id = "responsibleUserId3",
                        FirstName = "Responsible3",
                        LastName = "User3"
                    },
                    ImageName = "imageUrl",
                    Name = "Some event",
                    Place = "Some place",
                    MaxParticipants = 10,
                    OrganizationId = 2,
                    EventParticipants = new List<EventParticipant> { participant2 },
                    EventOptions = new List<EventOption>(),
                    EventType = new EventType
                    {
                        Id = 3,
                        IsShownWithMainEvents = true
                    },
                    Offices = "[\"1\", \"2\"]"
                },
                new Event
                {
                    Id = guids[3],
                    StartDate = DateTime.UtcNow.AddDays(-3),
                    EndDate = DateTime.UtcNow.AddDays(-3),
                    Created = DateTime.UtcNow,
                    EventTypeId = 3,
                    ResponsibleUserId = "responsibleUserId4",
                    ResponsibleUser = new ApplicationUser
                    {
                        Id = "responsibleUserId4",
                        FirstName = "Responsible4",
                        LastName = "User4"
                    },
                    ImageName = "imageUrl",
                    Name = "Some event",
                    Place = "Some place",
                    MaxParticipants = 10,
                    OrganizationId = 2,
                    EventParticipants = new List<EventParticipant> { participant2 },
                    EventOptions = new List<EventOption>(),
                    EventType = new EventType
                    {
                        Id = 3,
                        IsShownWithMainEvents = true
                    },
                    Offices = "[\"1\"]"
                },
                new Event
                {
                    Id = guids[4],
                    StartDate = DateTime.UtcNow.AddDays(10),
                    EndDate = DateTime.UtcNow.AddDays(21),
                    Created = DateTime.UtcNow,
                    EventTypeId = 3,
                    ResponsibleUserId = "responsibleUserId5",
                    ResponsibleUser = new ApplicationUser
                    {
                        Id = "responsibleUserId5",
                        FirstName = "Responsible5",
                        LastName = "User5"
                    },
                    ImageName = "imageUrl",
                    Name = "Cool event",
                    Place = "Cool place",
                    MaxParticipants = 10,
                    OrganizationId = 2,
                    EventParticipants = new List<EventParticipant> { participant4, participant5 },
                    EventOptions = new List<EventOption>(),
                    EventType = new EventType
                    {
                        Id = 3,
                        IsShownWithMainEvents = true
                    },
                    Offices = "[\"1\"]"
                },
                new Event
                {
                    Id = guids[5],
                    StartDate = DateTime.UtcNow.AddDays(10),
                    EndDate = DateTime.UtcNow.AddDays(21),
                    Created = DateTime.UtcNow,
                    EventTypeId = 3,
                    ResponsibleUserId = "responsibleUserId6",
                    ResponsibleUser = new ApplicationUser
                    {
                        Id = "responsibleUserId6",
                        FirstName = "Responsible6",
                        LastName = "User6"
                    },
                    ImageName = "imageUrl",
                    Name = "Cool event",
                    Place = "Cool place",
                    MaxParticipants = 10,
                    OrganizationId = 3,
                    EventParticipants = new List<EventParticipant> { participant6 },
                    EventOptions = new List<EventOption>(),
                    EventType = new EventType
                    {
                        Id = 3,
                        IsShownWithMainEvents = true
                    },
                    Offices = "[\"2\"]"
                }
            };

            var offices = new List<Office>
            {
                new Office
                {
                    Id = 1,
                    Name = "First office"
                },

                new Office
                {
                    Id = 2,
                    Name = "Second office"
                }
            };


            _eventsDbSet.SetDbSetDataForAsync(events);
            _eventParticipantsDbSet.SetDbSetDataForAsync(new List<EventParticipant>
            {
                participant1,
                participant2,
                participant3,
                participant4,
                participant5,
                participant6
            });
            _officeDbSet.SetDbSetDataForAsync(offices);

            return guids;
        }

        #endregion
    }
}
