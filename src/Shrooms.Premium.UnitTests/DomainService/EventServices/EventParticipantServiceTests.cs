using NSubstitute;
using NUnit.Framework;
using Shrooms.Constants.Authorization;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Events;
using Shrooms.Domain.Services.Email.Event;
using Shrooms.Domain.Services.Events.Calendar;
using Shrooms.Domain.Services.Events.Participation;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.Wall;
using Shrooms.DomainExceptions.Exceptions.Event;
using Shrooms.DomainServiceValidators.Validators.Events;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Infrastructure.SystemClock;
using Shrooms.UnitTests.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using static Shrooms.Premium.Other.Shrooms.Constants.ErrorCodes.ErrorCodes;

namespace Shrooms.UnitTests.DomainService.EventServices
{
    [TestFixture]
    public class EventParticipantServiceTests
    {
        private IDbSet<Event> _eventsDbSet;
        private IDbSet<ApplicationUser> _usersDbSet;
        private IDbSet<EventOption> _eventOptionsDbSet;
        private IDbSet<EventParticipant> _eventParticipantsDbSet;

        private IUnitOfWork2 _uow2;
        private ISystemClock _systemClockMock;
        private IEventParticipationService _eventParticipationService;
        private IEventValidationService _eventValidationServiceMock;
        private EventValidationService _eventValidationService;

        private IWallService _wallService;

        [SetUp]
        public void TestInitializer()
        {
            _uow2 = Substitute.For<IUnitOfWork2>();

            _eventsDbSet = Substitute.For<IDbSet<Event>>();
            _uow2.GetDbSet<Event>().Returns(_eventsDbSet);

            _usersDbSet = Substitute.For<IDbSet<ApplicationUser>>();
            _uow2.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _eventParticipantsDbSet = Substitute.For<IDbSet<EventParticipant>>();
            _uow2.GetDbSet<EventParticipant>().Returns(_eventParticipantsDbSet);

            _eventOptionsDbSet = Substitute.For<IDbSet<EventOption>>();
            _uow2.GetDbSet<EventOption>().Returns(_eventOptionsDbSet);

            _systemClockMock = Substitute.For<ISystemClock>();
            var calendarService = Substitute.For<IEventCalendarService>();
            var permissionService = Substitute.For<IPermissionService>();
            var eventNotificationService = Substitute.For<IEventNotificationService>();
            _eventValidationServiceMock = Substitute.For<IEventValidationService>();
            _eventValidationService = new EventValidationService(_systemClockMock);
            var roleService = Substitute.For<IRoleService>();
            MockRoleService(roleService);
            _wallService = Substitute.For<IWallService>();

            _eventParticipationService = 
                new EventParticipationService(
                    _uow2, 
                    _systemClockMock, 
                    roleService, 
                    permissionService, 
                    calendarService, 
                    _eventValidationServiceMock, 
                    eventNotificationService, 
                    _wallService);
        }

        [Test]
        public void Should_Successfully_Join_Event_Without_Options()
        {
            var eventGuid = MockEventWithoutOptions();
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));
            var eventJoinDto = new EventJoinDTO
            {
                ChosenOptions = new List<int>(),
                ParticipantIds = new List<string>() { "testUserId" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };
            _eventParticipationService.Join(eventJoinDto);
            _eventParticipantsDbSet.Received(1).Add(Arg.Any<EventParticipant>());
        }

        [Test]
        public void Should_Successfully_Join_Event_With_Options()
        {
            var eventGuid = MockEventWithOptions();
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-03-28"));
            var eventJoinDto = new EventJoinDTO
            {
                ChosenOptions = new List<int> { 1 },
                ParticipantIds = new List<string>() { "testUserId" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };
            _eventParticipationService.Join(eventJoinDto);
            _eventParticipantsDbSet.Received(1).Add(Arg.Is<EventParticipant>(x => x.EventOptions.Count == 1));
        }

        [Test]
        public void Should_Throw_If_Joining_Event_Is_Expired()
        {
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-06-21"));

            var registrationDeadline = DateTime.Parse("2016-06-20");
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfRegistrationDeadlineIsExpired(registrationDeadline));
            Assert.That(ex.Message, Is.EqualTo(EventRegistrationDeadlineIsExpired));
        }

        [Test]
        public void Should_Not_Throw_If_Joining_User_Is_Not_Participating_In_Event()
        {
            var isAlreadyParticipating = false;
            Assert.DoesNotThrow(() => _eventValidationService.CheckIfUserAlreadyJoinedSameEvent(isAlreadyParticipating));
        }

        [Test]
        public void Should_Throw_If_Joining_User_Provided_Not_Enough_Options()
        {
            var maxChoices = 2;
            var choicesProvided = 0;
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfJoiningNotEnoughChoicesProvided(maxChoices, choicesProvided));
            Assert.That(ex.Message, Is.EqualTo(EventNotEnoughChoicesProvidedCode));
        }

        [Test]
        public void Should_Throw_If_Joining_User_Provided_Too_Many_Choices()
        {
            var maxChoices = 2;
            var choicesProvided = 3;
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfJoiningTooManyChoicesProvided(maxChoices, choicesProvided));
            Assert.That(ex.Message, Is.EqualTo(EventTooManyChoicesProvidedCode));
        }

        [Test]
        public void Should_Throw_If_Joining_Event_Is_Already_Full()
        {
            var maxParticipants = 5;
            var participantsCount = 6;
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfEventHasEnoughPlaces(maxParticipants, participantsCount));
            Assert.That(ex.Message, Is.EqualTo(EventIsFullCode));
        }

        [Test]
        public void Should_Throw_If_Joining_Many_Users_One_Participant_Has_Already_Joined()
        {
            var isAlreadyParticipating = true;
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfUserAlreadyJoinedSameEvent(isAlreadyParticipating));
            Assert.That(ex.Message, Is.EqualTo(EventUserAlreadyParticipatesCode)); ;
        }

        [Test]
        public void Should_Return_If_Joining_Many_Users_Failed()
        {
            var eventGuid = MockEventWithOptions();
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-04-02"));
            var eventJoinDto = new EventJoinDTO
            {
                ParticipantIds = new List<string>() { "testUserId", "1" },
                ChosenOptions = new List<int>() { 1 },
                EventId = eventGuid,
                OrganizationId = 2
            };
            _eventParticipationService.Join(eventJoinDto);
            _eventParticipantsDbSet.Received(2).Add(Arg.Is<EventParticipant>(x => x.EventOptions.Count == 1));
            _uow2.Received(1).SaveChanges(false);
        }

        [Test]
        public void Should_Return_Participants_List()
        {
            var eventGuid = MockEventWithParticipants();
            MockUsers();
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };

            var result = _eventParticipationService.GetEventParticipants(eventGuid, userAndOrg);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Name", result.First().FirstName);
        }

        [Test]
        public void Should_Throw_When_Paricipant_Was_Not_Found()
        {
            MockLeaveEvent();
            var eventId = new Guid();
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
                UserId = "user"
            };
            Assert.Throws<EventException>(() => _eventParticipationService.Leave(eventId, userOrg));
        }

        [Test]
        public void Should_Leave_Event_That_Has_Not_Started()
        {
            var eventId = MockLeaveEvent();
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
                UserId = "user"
            };
            _eventParticipationService.Leave(eventId, userOrg);
            _eventParticipantsDbSet.Received(1).Remove(Arg.Any<EventParticipant>());
        }

        [Test]
        public void Should_Throw_When_Participant_Wasnt_removed()
        {
            var eventId = MockRemoveParticipant();
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
                UserId = "user"
            };

            _eventParticipationService.Expel(eventId, userOrg, "user2");
            _eventParticipantsDbSet.Received(1).Remove(Arg.Any<EventParticipant>());
        }

        [Test]
        public void Should_Throw_When_User_Has_No_Permission_To_Remove_Participant()
        {
            var eventId = MockRemoveParticipant();
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
                UserId = "user"
            };

            _eventParticipationService.Expel(eventId, userOrg, "user2");
            _eventValidationServiceMock.Received(1).CheckIfUserHasPermission("user", "user", false);
        }

        [Test]
        public void Should_Throw_If_Trying_To_Expell_Participant_From_Finished_Event()
        {
            var eventId = MockRemoveParticipantWithExpiredEvent();
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
                UserId = "user"
            };

            _eventParticipationService.Expel(eventId, userOrg, "user2");
            _eventValidationServiceMock.Received(1).CheckIfEventEndDateIsExpired(Arg.Is<DateTime>(x => x < DateTime.UtcNow));
        }

        [Test]
        public void Should_Throw_If_Participant_Does_Not_Exist()
        {
            EventParticipant noParticipant = null;
            Assert.Throws<EventException>(() => _eventValidationService.CheckIfParticipantExists(noParticipant));
        }

        [Test]
        public void Should_Not_Throw_If_Participant_Exists()
        {
            EventParticipant participant = new EventParticipant();
            Assert.DoesNotThrow(() => _eventValidationService.CheckIfParticipantExists(participant));
        }

        [Test]
        public void Should_Reset_Event_Participants()
        {
            var eventId = MockResetAttendees();
            var user = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
                UserId = "user1"
            };
            _eventParticipationService.ResetAttendees(eventId, user);
            _eventParticipantsDbSet.Received(3).Remove(Arg.Any<EventParticipant>());
        }

        [Test]
        public void Should_Throw_If_User_Is_Not_Creator_Or_Event_Admin()
        {
            var userId = "user";
            var eventCreatorId = "creator";
            var IsAdmin = false;
            Assert.Throws<EventException>(() => _eventValidationService.CheckIfUserHasPermission(userId, eventCreatorId, IsAdmin));
        }

        [Test]
        public void Should_Not_Throw_If_User_Is_Creator_And_Not_Event_Admin()
        {
            var userId = "creator";
            var eventCreatorId = "creator";
            var IsAdmin = false;
            Assert.DoesNotThrow(() => _eventValidationService.CheckIfUserHasPermission(userId, eventCreatorId, IsAdmin));
        }

        [Test]
        public void Should_Not_Throw_If_User_Is_Event_Administrator()
        {
            var userId = "user";
            var eventCreatorId = "creator";
            var IsAdmin = true;
            Assert.DoesNotThrow(() => _eventValidationService.CheckIfUserHasPermission(userId, eventCreatorId, IsAdmin));
        }

        [Test]
        public void Should_Return_Only_Not_Participating_Users_While_Searching_For_Users_To_Join_Event()
        {
            var eventId = MockEventAndUsersForSearch();
            var searchString = "user";
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };
            var result = _eventParticipationService.SearchForEventJoinAutocomplete(eventId, searchString, userOrg);
            Assert.AreEqual(3, result.Count());
            Assert.IsFalse(result.Any(x => x.Id == "user1"));
            Assert.IsFalse(result.Any(x => x.Id == "user2"));
        }

        [Test]
        public void Should_Search_By_UserName()
        {
            var eventId = MockEventAndUsersForSearch2();
            var searchString = "clyde";
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };
            var result = _eventParticipationService.SearchForEventJoinAutocomplete(eventId, searchString, userOrg);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("user4", result.First().Id);
        }

        [Test]
        public void Should_Search_By_FirstName()
        {
            var eventId = MockEventAndUsersForSearch2();
            var searchString = "User1f";
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };
            var result = _eventParticipationService.SearchForEventJoinAutocomplete(eventId, searchString, userOrg);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("user1", result.First().Id);
        }

        [Test]
        public void Should_Search_By_LastName()
        {
            var eventId = MockEventAndUsersForSearch2();
            var searchString = "User1l";
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };
            var result = _eventParticipationService.SearchForEventJoinAutocomplete(eventId, searchString, userOrg);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("user1", result.First().Id);
        }

        [Test]
        public void Should_Search_By_FullName()
        {
            var eventId = MockEventAndUsersForSearch2();
            var searchString = "User1fname user1l";
            var userOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };
            var result = _eventParticipationService.SearchForEventJoinAutocomplete(eventId, searchString, userOrg);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("user1", result.First().Id);
        }

        #region Mocks

        private static void MockRoleService(IRoleService roleService)
        {
            roleService.ExcludeUsersWithRole(Roles.NewUser).Returns(x => true);
        }

        private Guid MockEventWithParticipants()
        {
            var guid = new Guid();
            var @event = new List<Event>
            {
                new Event
                {
                    Id = guid,
                    OrganizationId = 2,
                    Name = "Test event",
                    EventParticipants = new List<EventParticipant>()
                    {
                        new EventParticipant
                        {
                            ApplicationUser = new ApplicationUser
                            {
                                FirstName = "Name",
                                LastName = "Surname"
                            }
                        },
                        new EventParticipant
                        {
                            ApplicationUser = new ApplicationUser
                            {
                                FirstName = "Name1",
                                LastName = "Surname1"
                            }
                        },
                    },
                }
            };
            _eventsDbSet.SetDbSetData(@event.AsQueryable());
            return guid;
        }

        public Guid MockEventAndUsersForSearch2()
        {
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Id = eventId,
                EventParticipants = new List<EventParticipant>(),
                OrganizationId = 2
            };

            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "user1",
                    FirstName = "user1fname",
                    LastName = "user1lname",
                    OrganizationId = 2,
                    UserName = "john"
                },
                new ApplicationUser
                {
                    Id = "user2",
                    FirstName = "user2fname",
                    LastName = "user2lname",
                    OrganizationId = 2,
                    UserName = "Peter"
                },
                new ApplicationUser
                {
                    Id = "user3",
                    FirstName = "user3fname",
                    LastName = "user3lname",
                    OrganizationId = 2,
                    UserName = "Andrew"
                },
                new ApplicationUser
                {
                    Id = "user4",
                    FirstName = "user4fname",
                    LastName = "user4lname",
                    OrganizationId = 2,
                    UserName = "Clyde"
                },
                new ApplicationUser
                {
                    Id = "user5",
                    FirstName = "user5fname",
                    LastName = "user5lname",
                    OrganizationId = 2,
                    UserName = "Sam"
                },
            };

            _eventsDbSet.SetDbSetData(new List<Event> { @event }.AsQueryable());
            _usersDbSet.SetDbSetData(users.AsQueryable());
            return eventId;
        }

        public Guid MockEventAndUsersForSearch()
        {
            var eventId = Guid.NewGuid();
            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    ApplicationUserId = "user1"
                },
                new EventParticipant
                {
                    ApplicationUserId = "user2"
                }
            };
            var @event = new Event
            {
                Id = eventId,
                EventParticipants = participants,
                OrganizationId = 2
            };

            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "user1",
                    FirstName = "user1",
                    LastName = "user1",
                    OrganizationId = 2,
                    UserName = "john"
                },
                new ApplicationUser
                {
                    Id = "user2",
                    FirstName = "user2",
                    LastName = "user2",
                    OrganizationId = 2,
                    UserName = "Peter"
                },
                new ApplicationUser
                {
                    Id = "user3",
                    FirstName = "user3",
                    LastName = "user3",
                    OrganizationId = 2,
                    UserName = "Andrew"
                },
                new ApplicationUser
                {
                    Id = "user4",
                    FirstName = "user4",
                    LastName = "user4",
                    OrganizationId = 2,
                    UserName = "Clyde"
                },
                new ApplicationUser
                {
                    Id = "user5",
                    FirstName = "user5",
                    LastName = "user5",
                    OrganizationId = 2,
                    UserName = "Sam"
                },
            };

            _eventsDbSet.SetDbSetData(new List<Event> { @event }.AsQueryable());
            _usersDbSet.SetDbSetData(users.AsQueryable());
            return eventId;
        }

        private Guid MockResetAttendees()
        {
            var eventId = Guid.NewGuid();

            var @event = new Event
            {
                Name = "test",
                Id = eventId,
                RegistrationDeadline = DateTime.Parse("2016-06-20"),
                OrganizationId = 2
            };

            var option1 = new EventOption
            {
                Id = 1,
                EventId = eventId,
                Event = @event,
                Option = "Option1"
            };
            var option2 = new EventOption
            {
                Id = 2,
                EventId = eventId,
                Event = @event,
                Option = "Option2"
            };
            var option3 = new EventOption
            {
                Id = 3,
                EventId = eventId,
                Event = @event,
                Option = "Option3",
            };

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = @event,
                    EventOptions = new List<EventOption> { option1 },
                    EventId = eventId,
                    ApplicationUserId = "user1",
                },
                new EventParticipant
                {
                    Id = 2,
                    Event = @event,
                    EventOptions = new List<EventOption> { option2 },
                    EventId = eventId,
                    ApplicationUserId = "user2"
                },
                new EventParticipant
                {
                    Id = 3,
                    Event = @event,
                    EventOptions = new List<EventOption> { option3 },
                    EventId = eventId,
                    ApplicationUserId = "user3"
                }
            };
            @event.EventOptions = new List<EventOption> { option1, option2, option3 };
            @event.EventParticipants = participants;

            _eventsDbSet.SetDbSetData(new List<Event> { @event }.AsQueryable());
            return eventId;
        }

        private Guid MockLeaveEvent()
        {
            _eventValidationServiceMock.When(x => x.CheckIfParticipantExists(null)).Do(x => { throw new EventException("Exception"); });
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Name = "test",
                Id = eventId,
                RegistrationDeadline = DateTime.Parse("2016-06-20"),
                OrganizationId = 2
            };

            var eventOptions = new List<EventOption>
            {
                new EventOption
                {
                    Id = 1,
                    EventId = eventId,
                    Event = @event,
                    Option = "Option1"
                },
                new EventOption
                {
                    Id = 2,
                    EventId = eventId,
                    Event = @event,
                    Option = "Option2"
                },
            };

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = @event,
                    EventOptions = eventOptions,
                    EventId = eventId,
                    ApplicationUserId = "user"
                }
            };

            _eventParticipantsDbSet.SetDbSetData(participants.AsQueryable());
            return eventId;
        }

        private Guid MockRemoveParticipantWithExpiredEvent()
        {
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Name = "test",
                Id = eventId,
                RegistrationDeadline = DateTime.Parse("2016-06-20"),
                EndDate = DateTime.Parse("2015-01-01"),
                OrganizationId = 2,
                ResponsibleUserId = "user"
            };

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = @event,
                    EventOptions = new List<EventOption>(),
                    EventId = eventId,
                    ApplicationUserId = "user"
                },
                new EventParticipant
                {
                    Id = 2,
                    Event = @event,
                    EventOptions = new List<EventOption>(),
                    EventId = eventId,
                    ApplicationUserId = "user2"
                }
            };

            _eventParticipantsDbSet.SetDbSetData(participants.AsQueryable());
            return eventId;
        }

        private Guid MockRemoveParticipant()
        {
            var eventId = Guid.NewGuid();
            var @event = new Event
            {
                Name = "test",
                Id = eventId,
                RegistrationDeadline = DateTime.Parse("2016-06-20"),
                OrganizationId = 2,
                ResponsibleUserId = "user"
            };

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = @event,
                    EventOptions = new List<EventOption>(),
                    EventId = eventId,
                    ApplicationUserId = "user"
                },
                new EventParticipant
                {
                    Id = 2,
                    Event = @event,
                    EventOptions = new List<EventOption>(),
                    EventId = eventId,
                    ApplicationUserId = "user2"
                }
            };

            _eventParticipantsDbSet.SetDbSetData(participants.AsQueryable());
            return eventId;
        }

        private Guid MockEventWithoutOptions()
        {
            var guid = Guid.NewGuid();
            var eventt = new List<Event>
            {
                new Event
                {
                    StartDate = DateTime.Parse("2016-04-05"),
                    EndDate = DateTime.Parse("2016-04-06"),
                    RegistrationDeadline = DateTime.Parse("2016-04-05"),
                    EventOptions = new List<EventOption>(),
                    Id = guid,
                    MaxChoices = 0,
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
                    EventTypeId = 1
                }
            };
            _eventsDbSet.SetDbSetData(eventt.AsQueryable());
            return guid;
        }

        private Guid MockEventWithOptions()
        {
            var guid = Guid.NewGuid();
            var eventt = new List<Event>
            {
                new Event
                {
                    StartDate = DateTime.Parse("2016-04-05"),
                    EndDate = DateTime.Parse("2016-04-07"),
                    RegistrationDeadline = DateTime.Parse("2016-04-05"),
                    EventOptions = new List<EventOption>
                    {
                        new EventOption
                        {
                            Id = 1,
                            EventId = guid,
                            Option = "Option1"
                        },
                        new EventOption
                        {
                            Id = 2,
                            EventId = guid,
                            Option = "Option2"
                        },
                        new EventOption
                        {
                            Id = 3,
                            EventId = guid,
                            Option = "Option3"
                        }
                    },
                    Id = guid,
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
                    EventTypeId = 1
                }
            };
            _eventsDbSet.SetDbSetData(eventt.AsQueryable());
            return guid;
        }

        private Guid MockEventWithOptionsAndParticipant()
        {
            var guid = Guid.NewGuid();
            var eventt = new List<Event>
            {
                new Event
                {
                    StartDate = DateTime.Parse("2016-04-05"),
                    EndDate = DateTime.Parse("2016-04-06"),
                    RegistrationDeadline = DateTime.Parse("2016-04-05"),
                    EventOptions = new List<EventOption>
                    {
                        new EventOption
                        {
                            Id = 1,
                            EventId = guid,
                            Option = "Option1"
                        },
                        new EventOption
                        {
                            Id = 2,
                            EventId = guid,
                            Option = "Option2"
                        },
                        new EventOption
                        {
                            Id = 3,
                            EventId = guid,
                            Option = "Option3"
                        }
                    },
                    Id = guid,
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
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant
                        {
                            Id = 1,
                            ApplicationUserId = "testUserId",
                            EventId = guid
                        }
                    },
                    EventTypeId = 1
                }
            };
            _eventsDbSet.SetDbSetData(eventt.AsQueryable());
            return guid;
        }

        private Guid MockFullEvent()
        {
            var guid = Guid.NewGuid();
            var eventt = new List<Event>
            {
                new Event
                {
                    StartDate = DateTime.Parse("2016-04-05"),
                    EndDate = DateTime.Parse("2016-04-06"),
                    RegistrationDeadline = DateTime.Parse("2016-04-05"),
                    EventOptions = new List<EventOption>(),
                    Id = guid,
                    MaxChoices = 0,
                    MaxParticipants = 3,
                    OrganizationId = 2,
                    Name = "Test event",
                    EventType = new EventType
                    {
                        Name = "test type",
                        IsSingleJoin = false,
                        Id = 1
                    },
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant
                        {
                            Id = 1,
                            ApplicationUserId = "testUserId1",
                            EventId = guid
                        },
                        new EventParticipant
                        {
                            Id = 2,
                            ApplicationUserId = "testUserId2",
                            EventId = guid
                        },
                        new EventParticipant
                        {
                            Id = 3,
                            ApplicationUserId = "testUserId3",
                            EventId = guid
                        }
                    },
                    EventTypeId = 1
                }
            };
            _eventsDbSet.SetDbSetData(eventt.AsQueryable());
            return guid;
        }

        private void MockExpiredEvent()
        {
            var eventt = new List<Event>
            {
                new Event
                {
                    StartDate = DateTime.Parse("2016-04-05"),
                    EndDate = DateTime.Parse("2016-04-07"),
                    RegistrationDeadline = DateTime.Parse("2016-04-05"),
                    EventOptions = new List<EventOption>
                    {
                        new EventOption
                        {
                            Id = 1,
                            EventId = Guid.Parse("1"),
                            Option = "Option1"
                        },
                        new EventOption
                        {
                            Id = 2,
                            EventId = Guid.Parse("1"),
                            Option = "Option2"
                        },
                        new EventOption
                        {
                            Id = 3,
                            EventId = Guid.Parse("1"),
                            Option = "Option3"
                        }
                    },
                    Id = Guid.Parse("1"),
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
                    EventTypeId = 1
                }
            };
            _eventsDbSet.SetDbSetData(eventt.AsQueryable());
        }

        private void MockUsers()
        {
            var types = new List<ApplicationUser>
            {
               new ApplicationUser()
               {
                   Id = "1",
                   OrganizationId = 2
               },
               new ApplicationUser()
               {
                   Id = "testUserId",
                   OrganizationId = 2
               }
            };
            _usersDbSet.SetDbSetData(types.AsQueryable());
        }
        #endregion
    }
}
