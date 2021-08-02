using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.Wall;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Events.Participation;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    [TestFixture]
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public class EventParticipantServiceTests
    {
        private DbSet<Event> _eventsDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<EventOption> _eventOptionsDbSet;
        private DbSet<EventParticipant> _eventParticipantsDbSet;

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

            _eventsDbSet = Substitute.For<DbSet<Event>, IQueryable<Event>, IDbAsyncEnumerable<Event>>();
            _uow2.GetDbSet<Event>().Returns(_eventsDbSet);

            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IDbAsyncEnumerable<ApplicationUser>>();
            _uow2.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _eventParticipantsDbSet = Substitute.For<DbSet<EventParticipant>, IQueryable<EventParticipant>, IDbAsyncEnumerable<EventParticipant>>();
            _uow2.GetDbSet<EventParticipant>().Returns(_eventParticipantsDbSet);

            _eventOptionsDbSet = Substitute.For<DbSet<EventOption>, IQueryable<EventOption>, IDbAsyncEnumerable<EventOption>>();
            _uow2.GetDbSet<EventOption>().Returns(_eventOptionsDbSet);

            _systemClockMock = Substitute.For<ISystemClock>();
            var permissionService = Substitute.For<IPermissionService>();
            _eventValidationServiceMock = Substitute.For<IEventValidationService>();
            _eventValidationService = new EventValidationService(_systemClockMock);
            var roleService = Substitute.For<IRoleService>();
            MockRoleService(roleService);
            _wallService = Substitute.For<IWallService>();
            var asyncRunner = Substitute.For<IAsyncRunner>();

            _eventParticipationService =
                new EventParticipationService(
                    _uow2,
                    _systemClockMock,
                    roleService,
                    permissionService,
                    _eventValidationServiceMock,
                    _wallService,
                    asyncRunner);
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
                ParticipantIds = new List<string> { "testUserId" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };
            _eventParticipationService.JoinAsync(eventJoinDto);
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
                ParticipantIds = new List<string> { "testUserId" },
                EventId = eventGuid,
                UserId = "testUserId",
                OrganizationId = 2
            };
            _eventParticipationService.JoinAsync(eventJoinDto);
            _eventParticipantsDbSet.Received(1).Add(Arg.Is<EventParticipant>(x => x.EventOptions.Count == 1));
        }

        [Test]
        public void Should_Throw_If_Joining_Event_Is_Expired()
        {
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-06-21"));

            var registrationDeadline = DateTime.Parse("2016-06-20");
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfRegistrationDeadlineIsExpired(registrationDeadline));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventRegistrationDeadlineIsExpired));
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
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventNotEnoughChoicesProvidedCode));
        }

        [Test]
        public void Should_Throw_If_Joining_User_Provided_Too_Many_Choices()
        {
            const int maxChoices = 2;
            const int choicesProvided = 3;
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfJoiningTooManyChoicesProvided(maxChoices, choicesProvided));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventTooManyChoicesProvidedCode));
        }

        [Test]
        public void Should_Throw_If_Joining_Event_Is_Already_Full()
        {
            const int maxParticipants = 5;
            var participantsCount = 6;
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfEventHasEnoughPlaces(maxParticipants, participantsCount));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventIsFullCode));
        }

        [Test]
        public void Should_Throw_If_Joining_Many_Users_One_Participant_Has_Already_Joined()
        {
            var isAlreadyParticipating = true;
            var ex = Assert.Throws<EventException>(() => _eventValidationService.CheckIfUserAlreadyJoinedSameEvent(isAlreadyParticipating));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventUserAlreadyParticipatesCode));
        }

        [Test]
        public async Task Should_Return_If_Joining_Many_Users_Failed()
        {
            var eventGuid = MockEventWithOptions();
            MockUsers();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-04-02"));
            var eventJoinDto = new EventJoinDTO
            {
                ParticipantIds = new List<string> { "testUserId", "1" },
                ChosenOptions = new List<int> { 1 },
                EventId = eventGuid,
                OrganizationId = 2
            };

            await _eventParticipationService.JoinAsync(eventJoinDto);
            _eventParticipantsDbSet.Received(2).Add(Arg.Is<EventParticipant>(x => x.EventOptions.Count == 1));
            await _uow2.Received(1).SaveChangesAsync(false);
        }

        [Test]
        public async Task Should_Return_Participants_List()
        {
            var eventGuid = MockEventWithParticipants();
            MockUsers();
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };

            var result = (await _eventParticipationService.GetEventParticipantsAsync(eventGuid, userAndOrg)).ToList();
            Assert.AreEqual(2, result.Count);
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

            Assert.ThrowsAsync<EventException>(async () => await _eventParticipationService.LeaveAsync(eventId, userOrg, "leave comment"));
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
            _eventParticipationService.LeaveAsync(eventId, userOrg, "leave comment");
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

            _eventParticipationService.ExpelAsync(eventId, userOrg, "user2");
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

            _eventParticipationService.ExpelAsync(eventId, userOrg, "user2");
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

            _eventParticipationService.ExpelAsync(eventId, userOrg, "user2");
            _eventValidationServiceMock.Received(1).CheckIfEventEndDateIsExpired(Arg.Is<DateTime>(x => x < DateTime.UtcNow));
        }

        [Test]
        public void Should_Throw_If_Participant_Does_Not_Exist()
        {
            EventParticipant noParticipant = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<EventException>(() => _eventValidationService.CheckIfParticipantExists(noParticipant));
        }

        [Test]
        public void Should_Not_Throw_If_Participant_Exists()
        {
            var participant = new EventParticipant();
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
            _eventParticipationService.ResetAttendeesAsync(eventId, user);
            _eventParticipantsDbSet.Received(3).Remove(Arg.Any<EventParticipant>());
        }

        [Test]
        public void Should_Throw_If_User_Is_Not_Creator_Or_Event_Admin()
        {
            var userId = "user";
            var eventCreatorId = "creator";
            var isAdmin = false;
            Assert.Throws<EventException>(() => _eventValidationService.CheckIfUserHasPermission(userId, eventCreatorId, isAdmin));
        }

        [Test]
        public void Should_Not_Throw_If_User_Is_Creator_And_Not_Event_Admin()
        {
            var userId = "creator";
            var eventCreatorId = "creator";
            var isAdmin = false;
            Assert.DoesNotThrow(() => _eventValidationService.CheckIfUserHasPermission(userId, eventCreatorId, isAdmin));
        }

        [Test]
        public void Should_Not_Throw_If_User_Is_Event_Administrator()
        {
            var userId = "user";
            var eventCreatorId = "creator";
            var isAdmin = true;
            Assert.DoesNotThrow(() => _eventValidationService.CheckIfUserHasPermission(userId, eventCreatorId, isAdmin));
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
            var result = _eventParticipationService.SearchForEventJoinAutocomplete(eventId, searchString, userOrg).ToList();
            Assert.AreEqual(3, result.Count);
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
            var result = _eventParticipationService.SearchForEventJoinAutocomplete(eventId, searchString, userOrg).ToList();
            Assert.AreEqual(1, result.Count);
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
            var result = _eventParticipationService.SearchForEventJoinAutocomplete(eventId, searchString, userOrg).ToList();
            Assert.AreEqual(1, result.Count);
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
            var result = _eventParticipationService.SearchForEventJoinAutocomplete(eventId, searchString, userOrg).ToList();
            Assert.AreEqual(1, result.Count);
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
            var result = _eventParticipationService.SearchForEventJoinAutocomplete(eventId, searchString, userOrg).ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("user1", result.First().Id);
        }

        [Test]
        public void Should_Throw_When_Event_Not_Found()
        {
            var dto = new EventChangeOptionsDTO { EventId = Guid.Empty, OrganizationId = 1 };
            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { new Event { Id = Guid.NewGuid(), OrganizationId = 1 } }.AsQueryable());
            _eventValidationServiceMock
                .When(x => x.CheckIfEventExists((object)null))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventDoesNotExistCode));

            Assert.Throws<EventException>(() => _eventParticipationService.UpdateSelectedOptions(dto), PremiumErrorCodes.EventDoesNotExistCode);
        }

        [Test]
        public void Should_Throw_When_Registration_Deadline_Expired()
        {
            var guid = MockEventWithOptions();
            var dto = new EventChangeOptionsDTO { EventId = guid, OrganizationId = 2, ChosenOptions = new List<int> { 1 } };
            _eventValidationServiceMock
                .When(x => x.CheckIfRegistrationDeadlineIsExpired(DateTime.Parse("2016-04-05")))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventRegistrationDeadlineIsExpired));

            Assert.Throws<EventException>(() => _eventParticipationService.UpdateSelectedOptions(dto), PremiumErrorCodes.EventRegistrationDeadlineIsExpired);
        }

        [Test]
        public void Should_Throw_When_Chosen_Options_Invalid()
        {
            var guid = MockEventWithOptions();
            var chosenOptionIds = new List<int> { -9999 };
            var dto = new EventChangeOptionsDTO { EventId = guid, OrganizationId = 2, ChosenOptions = chosenOptionIds };

            _eventValidationServiceMock
                .When(x => x.CheckIfProvidedOptionsAreValid(chosenOptionIds, Arg.Is<ICollection<EventOption>>(a => a.Count == 0)))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventRegistrationDeadlineIsExpired));

            Assert.Throws<EventException>(() => _eventParticipationService.UpdateSelectedOptions(dto), PremiumErrorCodes.EventNoSuchOptionsCode);
        }

        [Test]
        public void Should_Throw_When_Not_Enough_Options_Chosen()
        {
            var guid = MockEventWithOptions();
            var chosenOptionIds = new List<int>();
            var dto = new EventChangeOptionsDTO { EventId = guid, OrganizationId = 2, ChosenOptions = chosenOptionIds };

            _eventValidationServiceMock
                .When(x => x.CheckIfJoiningNotEnoughChoicesProvided(1, 0))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventNotEnoughChoicesProvidedCode));

            Assert.Throws<EventException>(() => _eventParticipationService.UpdateSelectedOptions(dto), PremiumErrorCodes.EventNotEnoughChoicesProvidedCode);
        }

        [Test]
        public void Should_Throw_When_Too_Many_Options_Chosen()
        {
            var guid = MockEventWithOptions();
            var chosenOptionIds = new List<int> { 1, 2, 3 };
            var dto = new EventChangeOptionsDTO { EventId = guid, OrganizationId = 2, ChosenOptions = chosenOptionIds };

            _eventValidationServiceMock
                .When(x => x.CheckIfJoiningTooManyChoicesProvided(1, 3))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventTooManyChoicesProvidedCode));

            Assert.Throws<EventException>(() => _eventParticipationService.UpdateSelectedOptions(dto), PremiumErrorCodes.EventTooManyChoicesProvidedCode);
        }

        [Test]
        public void Should_Throw_When_Single_Join_Rule_Chosen_With_More_Options()
        {
            var guid = MockEventWithOptions();
            var chosenOptionIds = new List<int> { 1, 4 };
            var dto = new EventChangeOptionsDTO { EventId = guid, OrganizationId = 2, ChosenOptions = chosenOptionIds };

            _eventValidationServiceMock
                .When(x => x.CheckIfSingleChoiceSelectedWithRule(Arg.Is<ICollection<EventOption>>(a =>
                    a.Any(e => e.Rule == OptionRules.IgnoreSingleJoin) && a.Count > 1), OptionRules.IgnoreSingleJoin))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventChoiceCanBeSingleOnly));

            Assert.Throws<EventException>(() => _eventParticipationService.UpdateSelectedOptions(dto), PremiumErrorCodes.EventChoiceCanBeSingleOnly);
        }

        [Test]
        public void Should_Throw_When_User_Is_Not_Participating_In_Event()
        {
            var guid = MockEventWithOptions();
            var chosenOptionIds = new List<int> { 1 };
            var dto = new EventChangeOptionsDTO { EventId = guid, OrganizationId = 2, ChosenOptions = chosenOptionIds, UserId = "1foo2bar" };

            _eventValidationServiceMock
                .When(x => x.CheckIfUserParticipatesInEvent("1foo2bar",
                    Arg.Is<List<string>>(a => a.All(p => p == "user" || p == "user2"))))
                .Do(_ => throw new EventException(PremiumErrorCodes.EventUserNotParticipating));

            Assert.Throws<EventException>(() => _eventParticipationService.UpdateSelectedOptions(dto), PremiumErrorCodes.EventUserNotParticipating);
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
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant
                        {
                            ApplicationUser = new ApplicationUser
                            {
                                FirstName = "Name",
                                LastName = "Surname"
                            },
                            AttendStatus = 1
                        },
                        new EventParticipant
                        {
                            ApplicationUser = new ApplicationUser
                            {
                                FirstName = "Name1",
                                LastName = "Surname1"
                            },
                            AttendStatus = 1
                        }
                    }
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(@event.AsQueryable());
            return guid;
        }

        private Guid MockEventAndUsersForSearch2()
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
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event }.AsQueryable());
            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());
            return eventId;
        }

        private Guid MockEventAndUsersForSearch()
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
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event }.AsQueryable());
            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());
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
                Option = "Option3"
            };

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = @event,
                    EventOptions = new List<EventOption> { option1 },
                    EventId = eventId,
                    ApplicationUserId = "user1"
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

            _eventsDbSet.SetDbSetDataForAsync(new List<Event> { @event }.AsQueryable());
            return eventId;
        }

        private Guid MockLeaveEvent()
        {
            _eventValidationServiceMock.When(x => x.CheckIfParticipantExists(null)).Do(_ => throw new EventException("Exception"));
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
                }
            };

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = @event,
                    EventOptions = eventOptions,
                    EventId = eventId,
                    ApplicationUserId = "user",
                    AttendStatus = 1
                }
            };

            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.AsQueryable());
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
                    ApplicationUserId = "user",
                    AttendStatus = 1
                },
                new EventParticipant
                {
                    Id = 2,
                    Event = @event,
                    EventOptions = new List<EventOption>(),
                    EventId = eventId,
                    ApplicationUserId = "user2",
                    AttendStatus = 1
                }
            };

            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.AsQueryable());
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
                    ApplicationUserId = "user",
                    AttendStatus = 1
                },
                new EventParticipant
                {
                    Id = 2,
                    Event = @event,
                    EventOptions = new List<EventOption>(),
                    EventId = eventId,
                    ApplicationUserId = "user2",
                    AttendStatus = 1
                }
            };

            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.AsQueryable());
            return eventId;
        }

        private Guid MockEventWithoutOptions()
        {
            var guid = Guid.NewGuid();
            var @event = new List<Event>
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

            var defaultEvent = @event.First();

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = defaultEvent,
                    EventOptions = new List<EventOption>(),
                    EventId = defaultEvent.Id,
                    ApplicationUserId = "user",
                    AttendStatus = 1
                },
                new EventParticipant
                {
                    Id = 2,
                    Event = defaultEvent,
                    EventOptions = new List<EventOption>(),
                    EventId = defaultEvent.Id,
                    ApplicationUserId = "user2",
                    AttendStatus = 1
                }
            };

            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.AsQueryable());
            _eventsDbSet.SetDbSetDataForAsync(@event.AsQueryable());
            return guid;
        }

        private Guid MockEventWithOptions()
        {
            var guid = Guid.NewGuid();

            var events = new List<Event>
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
                        },
                        new EventOption
                        {
                            Id = 4,
                            EventId = guid,
                            Option = "Option4",
                            Rule = OptionRules.IgnoreSingleJoin
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
                    EventTypeId = 1
                }
            };

            var defaultEvent = events.First();

            var participants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    Event = defaultEvent,
                    EventOptions = new List<EventOption>(),
                    EventId = defaultEvent.Id,
                    ApplicationUserId = "user",
                    AttendStatus = 1
                },
                new EventParticipant
                {
                    Id = 2,
                    Event = defaultEvent,
                    EventOptions = new List<EventOption>(),
                    EventId = defaultEvent.Id,
                    ApplicationUserId = "user2",
                    AttendStatus = 1
                }
            };

            foreach (var @event in events)
            {
                @event.EventParticipants = participants;
            }

            _eventParticipantsDbSet.SetDbSetDataForAsync(participants.AsQueryable());
            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());
            return guid;
        }

        private void MockUsers()
        {
            var types = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "1",
                    OrganizationId = 2
                },
                new ApplicationUser
                {
                    Id = "testUserId",
                    OrganizationId = 2
                }
            };
            _usersDbSet.SetDbSetDataForAsync(types.AsQueryable());
        }

        #endregion
    }
}
