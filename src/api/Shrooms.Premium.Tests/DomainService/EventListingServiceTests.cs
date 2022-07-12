using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
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

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(result.First(x => x.Id == eventGuids[0]).ParticipatingStatus, 1);
            Assert.IsTrue(result.First(x => x.Id == eventGuids[2]).StartDate < result.First(x => x.Id == eventGuids[0]).StartDate);
        }

        [Test]
        public async Task Should_Return_My_Events_As_A_Master()
        {
            var eventGuids = MockEventsListTest();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "responsibleUserId2",
            };

            var myEventsOptions = new MyEventsOptionsDto
            {
                SearchString = null,
                Filter = MyEventsOptions.Host,
                Page = 1,
                PageSize = 10
            };

            var result = (await _eventListingService.GetMyEventsAsync(myEventsOptions, userOrg)).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.First(x => x.Id == eventGuids[3]).IsCreator);
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
            Assert.IsInstanceOf<IPagedList>(result);
            Assert.That(result, Is.All.Matches<EventDetailsListItemDto>(item => item.Name.Contains(reportListingArgs.SearchString)));
            Assert.That(result, Is.All.Matches<EventDetailsListItemDto>(item => item.StartDate > DateTime.UtcNow));
        }

        [Test]
        public async Task Should_Return_All_Not_Started_Events_If_Search_String_Is_Not_Provided()
        {
            // Arrange
            MockEventsListTest();

            var expectedCount = 3;

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
            Assert.IsInstanceOf<IPagedList>(result);
            Assert.AreEqual(expectedCount, result.Count);
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
            Assert.IsInstanceOf<IPagedList>(result);
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
            Assert.IsInstanceOf<IPagedList>(result);
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
            Assert.IsInstanceOf<IPagedList>(result);
            Assert.That(result, Is.All.Matches<EventDetailsListItemDto>(item => item.StartDate > DateTime.UtcNow));
            CollectionAssert.AreEqual(expectedEventIdsByNameOrder, result.Select(item => item.Id));
        }

        [Test]
        public async Task Should_Return_Report_Participants()
        {
            // Arrange
            var eventWithParticipants = MockEventReportParticipantsTest()[0];
            var expectedParticipantCount = 2;

            var reportListingArgs = new EventParticipantsReportListingArgsDto
            {
                EventId = eventWithParticipants.Id,
                KudosTypeIds = Enumerable.Empty<int>(),
                EventTypeIds = Enumerable.Empty<int>(),
                Page = 1,
                PageSize = 10
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 3,
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _eventListingService.GetReportParticipantsAsync(reportListingArgs, userOrg);

            // Arrange
            Assert.AreEqual(expectedParticipantCount, result.Count());
        }

        [Test]
        public async Task Should_Return_Report_Participants_With_Filtered_VisitedEvents()
        {
            // Arrange
            var eventWithParticipants = MockEventReportParticipantsTest()[0];
            var expectedVisitedEventsCount = 2;

            var reportListingArgs = new EventParticipantsReportListingArgsDto
            {
                EventId = eventWithParticipants.Id,
                KudosTypeIds = Enumerable.Empty<int>(),
                EventTypeIds = new List<int> { 1 },
                Page = 1,
                PageSize = 10
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 3,
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _eventListingService.GetReportParticipantsAsync(reportListingArgs, userOrg);

            // Arrange
            Assert.AreEqual(expectedVisitedEventsCount, result.First().VisitedEvents.Count);
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
                PageSize = 10
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
                PageSize = 10
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
            var expectedKudosPoints = 30;
            var reportListingArgs = new EventParticipantsReportListingArgsDto
            {
                EventId = eventWithParticipants.Id,
                KudosTypeIds = Enumerable.Empty<int>(),
                EventTypeIds = Enumerable.Empty<int>(),
                SortByProperties = "Kudos desc",
                Page = 1,
                PageSize = 10
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 3,
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _eventListingService.GetReportParticipantsAsync(reportListingArgs, userOrg);

            // Arrange
            Assert.AreEqual(expectedKudosPoints, result.First().Kudos);
        }

        [Test]
        public async Task Should_Return_Report_Participants_With_Filtered_Kudos_And_Includes_Only_Approved()
        {
            // Arrange
            var eventWithParticipants = MockEventReportParticipantsTest()[0];
            var expectedKudosPoints = 20;
            var reportListingArgs = new EventParticipantsReportListingArgsDto
            {
                EventId = eventWithParticipants.Id,
                KudosTypeIds = new List<int> { 1 },
                EventTypeIds = Enumerable.Empty<int>(),
                SortByProperties = "Kudos desc",
                Page = 1,
                PageSize = 10
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 3,
                UserId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _eventListingService.GetReportParticipantsAsync(reportListingArgs, userOrg);

            // Arrange
            Assert.AreEqual(expectedKudosPoints, result.First().Kudos);
        }

        #region Mocks
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

            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());
            
            return guids;
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
                    EventParticipants = new List<EventParticipant>(),
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
                    EventParticipants = new List<EventParticipant>(),
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
                    EventParticipants = new List<EventParticipant>(),
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
                    }
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
                    }
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
                    }
                },
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
                AttendStatus = 3,
            };

            var participant4 = new EventParticipant
            {
                ApplicationUserId = "responsibleUserId4",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 4,
                EventId = guids[4],
                AttendStatus = 1,
                Event = new Event
                {
                },
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
                    Events = new List<Event>
                    {
                    }
                }
            };

            var participant5 = new EventParticipant
            {
                ApplicationUserId = "responsibleUserId5",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 5,
                EventId = guids[4],
                AttendStatus = 1,
                Event = new Event
                {
                },
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
                    Events = new List<Event>
                    {
                    }
                }
            };

            var participant6 = new EventParticipant
            {
                ApplicationUserId = "responsibleUserId6",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 6,
                EventId = guids[5],
                AttendStatus = 1,
                Event = new Event
                {
                },
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
                            Name = "Some kind of event",
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
                            Name = "Racing",
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
                            Name = "Running",
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
                    StartDate = DateTime.UtcNow.AddDays(10),
                    EndDate = DateTime.UtcNow.AddDays(21),
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
