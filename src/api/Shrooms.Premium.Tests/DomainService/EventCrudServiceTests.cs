﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Helpers;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Wall;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Events;
using Shrooms.Premium.Domain.Services.Events.Participation;
using Shrooms.Premium.Domain.Services.Events.Utilities;
using Shrooms.Premium.Domain.Services.OfficeMap;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    public class EventCrudServiceTests
    {
        private DbSet<Event> _eventsDbSet;
        private DbSet<Office> _officeDbSet;
        private DbSet<EventType> _eventTypesDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<EventOption> _eventOptionsDbSet;

        private IUnitOfWork2 _uow;
        private IWallService _wallService;
        private IEventService _eventService;
        private ISystemClock _systemClockMock;
        private IPermissionService _permissionService;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();

            _eventsDbSet = _uow.MockDbSetForAsync<Event>();
            _officeDbSet = _uow.MockDbSetForAsync<Office>();
            _eventOptionsDbSet = _uow.MockDbSetForAsync<EventOption>();
            _usersDbSet = _uow.MockDbSetForAsync<ApplicationUser>();
            _eventTypesDbSet = _uow.MockDbSetForAsync<EventType>();

            _permissionService = Substitute.For<IPermissionService>();
            _systemClockMock = Substitute.For<ISystemClock>();
            _wallService = Substitute.For<IWallService>();

            var eventParticipationService = Substitute.For<IEventParticipationService>();
            var eventUtilitiesService = Substitute.For<IEventUtilitiesService>();
            var eventValidationService = new EventValidationService(_systemClockMock);
            var officeMapService = Substitute.For<IOfficeMapService>();

            _eventService = new EventService(_uow,
                _permissionService,
                eventUtilitiesService,
                eventValidationService,
                eventParticipationService,
                _wallService,
                officeMapService);
        }

        [Test]
        public async Task Should_Delete_Event()
        {
            var eventId = MockEventDelete();
            var userAndOrganization = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "eventHostId"
            };

            await _eventService.DeleteAsync(eventId, userAndOrganization);
            _eventsDbSet.Received(1).Remove(Arg.Is<Event>(x => x.Id == eventId));
        }

        [Test]
        public async Task Should_Return_Event_For_Update()
        {
            var eventId = MockEventWithAllChildEntities();
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "userParticipant2"
            };

            var result = await _eventService.GetEventForEditingAsync(eventId, userOrg);
            Assert.AreEqual(2, result.Options.Count());
            Assert.NotNull(result.Location);
            Assert.NotNull(result.Name);
            Assert.NotNull(result.ImageName);
            Assert.NotNull(result.HostUserFullName);
            Assert.NotNull(result.HostUserId);
            Assert.AreEqual(1, result.MaxOptions);
            Assert.AreEqual(3, result.MaxParticipants);
            Assert.NotNull(result.Description);
        }

        [Test]
        public async Task Should_Return_Event_Types_For_Update()
        {
            var eventId = MockEventWithAllChildEntities();
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "userParticipant2"
            };

            var result = await _eventService.GetEventForEditingAsync(eventId, userOrg);
            Assert.AreEqual(3, result.TypeId);
        }

        [Test]
        public async Task Should_Return_Created_Event_Without_Options()
        {
            MockUsers();
            MockEventTypes();
            var newEvent = new CreateEventDto
            {
                EndDate = DateTime.UtcNow.AddHours(2),
                StartDate = DateTime.UtcNow.AddHours(1),
                Name = "Name",
                TypeId = 1,
                ImageName = "qwer",
                Offices = new EventOfficesDto { Value = "[\"1\"]", OfficeNames = new List<string> { "office" } },
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 0,
                MaxParticipants = 1,
                OrganizationId = 1,
                ResponsibleUserId = "1",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>()
            };

            await _eventService.CreateEventAsync(newEvent);
            _eventsDbSet.Received(1).Add(Arg.Any<Event>());
            _eventOptionsDbSet.Received(0).Add(Arg.Any<EventOption>());
        }

        [Test]
        public async Task Should_Return_Created_Event_With_Wall()
        {
            MockUsers();
            MockEventTypes();
            var newEvent = new CreateEventDto
            {
                EndDate = DateTime.UtcNow.AddHours(2),
                StartDate = DateTime.UtcNow.AddHours(1),
                Name = "Name",
                TypeId = 1,
                ImageName = "qwer",
                Offices = new EventOfficesDto { Value = "[\"1\"]", OfficeNames = new List<string> { "office" } },
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 0,
                MaxParticipants = 1,
                OrganizationId = 1,
                ResponsibleUserId = "1",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>()
            };

            await _eventService.CreateEventAsync(newEvent);
            await _wallService.Received(1)
                .CreateNewWallAsync(Arg.Is<CreateWallDto>(x =>
                    x.Name == newEvent.Name &&
                    x.Access == WallAccess.Private &&
                    x.Type == WallType.Events &&
                    x.ModeratorsIds.Count() == 1 &&
                    x.ModeratorsIds.Any(y => y == newEvent.ResponsibleUserId)));
        }

        [Test]
        public async Task Should_Return_Created_Event_With_Options()
        {
            MockUsers();
            MockEventTypes();
            var newEvent = new CreateEventDto
            {
                EndDate = DateTime.UtcNow.AddHours(2),
                StartDate = DateTime.UtcNow.AddHours(1),
                Name = "Name",
                TypeId = 1,
                ImageName = "qwer",
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 1,
                MaxParticipants = 1,
                Offices = new EventOfficesDto { Value = "[\"1\"]", OfficeNames = new List<string> { "office" } },
                OrganizationId = 1,
                ResponsibleUserId = "1",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>
                {
                    new NewEventOptionDto
                    {
                        Option = "Type1"
                    },
                    new NewEventOptionDto
                    {
                        Option = "Type2"
                    }
                }
            };

            await _eventService.CreateEventAsync(newEvent);
            _eventsDbSet.Received(1).Add(Arg.Any<Event>());
            _eventOptionsDbSet.Received(2).Add(Arg.Any<EventOption>());
        }

        [Test]
        public void Should_Return_Validation_Exception_For_Option_Duplicates()
        {
            MockUsers();
            MockEventTypes();
            var newEvent = new CreateEventDto
            {
                EndDate = DateTime.UtcNow.AddHours(2),
                StartDate = DateTime.UtcNow.AddHours(1),
                Name = "Name",
                TypeId = 1,
                ImageName = "qwer",
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 1,
                MaxParticipants = 1,
                OrganizationId = 1,
                ResponsibleUserId = "1",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>
                {
                    new NewEventOptionDto
                    {
                        Option = "Type1"
                    },
                    new NewEventOptionDto
                    {
                        Option = "Type1"
                    }
                }
            };
            var ex = Assert.ThrowsAsync<EventException>(async () => await _eventService.CreateEventAsync(newEvent));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventOptionsCantDuplicate));
        }

        [Test]
        public void Should_Return_User_Validation_Exception_While_Creating_Event()
        {
            MockUsers();
            MockEventTypes();
            var newEvent = new CreateEventDto
            {
                EndDate = DateTime.UtcNow.AddHours(2),
                StartDate = DateTime.UtcNow.AddHours(1),
                Name = "Name",
                TypeId = 1,
                ImageName = "qwer",
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 1,
                MaxParticipants = 1,
                OrganizationId = 1,
                ResponsibleUserId = "2",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>
                {
                    new NewEventOptionDto
                    {
                        Option = "Type1"
                    },
                    new NewEventOptionDto
                    {
                        Option = "Type2"
                    }
                }
            };
            var ex = Assert.ThrowsAsync<EventException>(async () => await _eventService.CreateEventAsync(newEvent));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventResponsiblePersonDoesNotExistCode));
        }

        [Test]
        public void Should_Return_Event_Type_Validation_Exception_While_Creating_Event()
        {
            MockUsers();
            MockEventTypes();
            var newEvent = new CreateEventDto
            {
                EndDate = DateTime.UtcNow.AddHours(2),
                StartDate = DateTime.UtcNow.AddHours(1),
                Name = "Name",
                TypeId = 100,
                ImageName = "qwer",
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 1,
                MaxParticipants = 1,
                OrganizationId = 1,
                ResponsibleUserId = "1",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>
                {
                    new NewEventOptionDto
                    {
                        Option = "Type1"
                    },
                    new NewEventOptionDto
                    {
                        Option = "Type2"
                    }
                }
            };

            var ex = Assert.ThrowsAsync<EventException>(async () => await _eventService.CreateEventAsync(newEvent));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventTypeDoesNotExistCode));
        }

        [Test]
        public void Should_Throw_Registration_Deadline_Expiration_Exception_While_Creating_Event()
        {
            MockUsers();
            MockEventTypes();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-04-05"));
            var newEvent = new CreateEventDto
            {
                EndDate = DateTime.Parse("2016-04-10"),
                StartDate = DateTime.Parse("2016-04-09"),
                RegistrationDeadlineDate = DateTime.Parse("2016-04-04"),
                Name = "Name",
                TypeId = 1,
                ImageName = "qwer",
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 1,
                MaxParticipants = 1,
                OrganizationId = 1,
                ResponsibleUserId = "1",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>
                {
                    new NewEventOptionDto
                    {
                        Option = "Type1"
                    },
                    new NewEventOptionDto
                    {
                        Option = "Type2"
                    }
                }
            };
            var ex = Assert.ThrowsAsync<EventException>(async () => await _eventService.CreateEventAsync(newEvent));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventRegistrationDeadlineIsExpired));
        }

        [Test]
        public void Should_Return_Registration_Deadline_Greater_Than_Start_Date_Exception_While_Creating_Event()
        {
            MockUsers();
            MockEventTypes();
            _systemClockMock.UtcNow.Returns(DateTime.Parse("2016-04-05"));
            var newEvent = new CreateEventDto
            {
                EndDate = DateTime.Parse("2016-04-10"),
                StartDate = DateTime.Parse("2016-04-06"),
                RegistrationDeadlineDate = DateTime.Parse("2016-04-07"),
                Name = "Name",
                TypeId = 1,
                ImageName = "qwer",
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 1,
                MaxParticipants = 1,
                OrganizationId = 1,
                ResponsibleUserId = "1",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>
                {
                    new NewEventOptionDto
                    {
                        Option = "Type1"
                    },
                    new NewEventOptionDto
                    {
                        Option = "Type2"
                    }
                }
            };
            var ex = Assert.ThrowsAsync<EventException>(async () => await _eventService.CreateEventAsync(newEvent));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventRegistrationDeadlineGreaterThanStartDateCode));
        }

        [Test]
        public void Should_Return_Start_End_Date_Validation_Exception_While_Creating_Event()
        {
            MockUsers();
            MockEventTypes();
            var newEvent = new CreateEventDto
            {
                EndDate = DateTime.UtcNow.AddHours(1),
                StartDate = DateTime.UtcNow.AddHours(2),
                Name = "Name",
                TypeId = 1,
                ImageName = "qwer",
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 1,
                MaxParticipants = 1,
                OrganizationId = 1,
                ResponsibleUserId = "1",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>
                {
                    new NewEventOptionDto
                    {
                        Option = "Type1"
                    },
                    new NewEventOptionDto
                    {
                        Option = "Type2"
                    }
                }
            };
            var ex = Assert.ThrowsAsync<EventException>(async () => await _eventService.CreateEventAsync(newEvent));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventStartDateGreaterThanEndDateCode));
        }

        [Test]
        public void Should_Return_MaxChoices_Validation_Exception_While_Creating_Event()
        {
            MockUsers();
            MockEventTypes();
            var newEvent = new CreateEventDto
            {
                EndDate = DateTime.UtcNow.AddHours(3),
                StartDate = DateTime.UtcNow.AddHours(2),
                Name = "Name",
                TypeId = 1,
                ImageName = "qwer",
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 0,
                MaxParticipants = 1,
                OrganizationId = 1,
                ResponsibleUserId = "1",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>
                {
                    new NewEventOptionDto
                    {
                        Option = "Type1"
                    },
                    new NewEventOptionDto
                    {
                        Option = "Type2"
                    }
                }
            };
            var ex = Assert.ThrowsAsync<EventException>(async () => await _eventService.CreateEventAsync(newEvent));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventNeedToHaveMaxChoiceCode));
        }

        [Test]
        public void Should_Return_No_Event_Exception_While_Updating_Event()
        {
            MockUsers();
            MockEventTypes();
            MockEventsListTest();
            var newEvent = new EditEventDto
            {
                Id = Guid.NewGuid(),
                ResetParticipantList = false,
                EndDate = DateTime.UtcNow.AddHours(3),
                StartDate = DateTime.UtcNow.AddHours(2),
                Name = "Name",
                TypeId = 1,
                ImageName = "qwer",
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 0,
                MaxParticipants = 1,
                OrganizationId = 1,
                ResponsibleUserId = "1",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>(),
                EditedOptions = new List<EventOptionDto>
                {
                    new EventOptionDto
                    {
                        Id = 1,
                        Option = "edited1"
                    },
                    new EventOptionDto
                    {
                        Id = 2,
                        Option = "edited2"
                    }
                }
            };
            var ex = Assert.ThrowsAsync<EventException>(async () => await _eventService.UpdateEventAsync(newEvent));
            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventDoesNotExistCode));
        }

        [Test]
        public void Should_Return_No_Permission_Exception_While_Updating_Event()
        {
            MockPermissionService(_permissionService);
            MockUsers();
            MockEventTypes();
            var eventsGuids = MockEventsListTest();
            var newEvent = new EditEventDto
            {
                Id = eventsGuids[0],
                UserId = "2",
                ResetParticipantList = false,
                EndDate = DateTime.UtcNow.AddHours(3),
                StartDate = DateTime.UtcNow.AddHours(2),
                Name = "Name",
                TypeId = 1,
                ImageName = "qwer",
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 0,
                MaxParticipants = 1,
                OrganizationId = 2,
                ResponsibleUserId = "1",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>(),
                EditedOptions = new List<EventOptionDto>
                {
                    new EventOptionDto
                    {
                        Id = 1,
                        Option = "edited1"
                    },
                    new EventOptionDto
                    {
                        Id = 2,
                        Option = "edited2"
                    }
                }
            };
            var ex = Assert.ThrowsAsync<EventException>(async () => await _eventService.UpdateEventAsync(newEvent));
            Assert.That(PremiumErrorCodes.EventDontHavePermissionCode, Is.EqualTo(ex.Message));
        }

        [Test]
        public async Task Should_Return_Updated_Event()
        {
            MockPermissionService(_permissionService);
            MockUsers();
            MockEventTypes();
            var eventsGuids = MockEventsListTest();
            MockEventOptions();

            var newEvent = new EditEventDto
            {
                Id = eventsGuids[0],
                UserId = "1",
                ResetParticipantList = false,
                EndDate = DateTime.UtcNow.AddHours(3),
                StartDate = DateTime.UtcNow.AddHours(2),
                Name = "Name",
                TypeId = 1,
                ImageName = "qwer",
                Offices = new EventOfficesDto { OfficeNames = new List<string> { "office" } },
                Recurrence = EventRecurrenceOptions.EveryDay,
                MaxOptions = 1,
                MaxParticipants = 1,
                OrganizationId = 2,
                ResponsibleUserId = "1",
                Location = "place",
                NewOptions = new List<NewEventOptionDto>
                {
                    new NewEventOptionDto
                    {
                        Option = "Type1"
                    },
                    new NewEventOptionDto
                    {
                        Option = "Type2"
                    }
                },
                EditedOptions = new List<EventOptionDto>
                {
                    new EventOptionDto
                    {
                        Id = 1,
                        Option = "Edited1"
                    },
                    new EventOptionDto
                    {
                        Id = 2,
                        Option = "Edited2"
                    }
                }
            };

            await _eventService.UpdateEventAsync(newEvent);

            _eventOptionsDbSet.Received(0).Remove(Arg.Any<EventOption>());
            _eventOptionsDbSet.Received(2).Add(Arg.Any<EventOption>());
            await _uow.Received(1).SaveChangesAsync(false);
        }

        private void MockEventTypes()
        {
            var types = new List<EventType>
            {
                new EventType
                {
                    Id = 1,
                    Name = "type1",
                    OrganizationId = 2
                },
                new EventType
                {
                    Id = 2,
                    Name = "type2",
                    OrganizationId = 2
                },
                new EventType
                {
                    Id = 3,
                    Name = "type3",
                    OrganizationId = 2
                },
                new EventType
                {
                    Id = 4,
                    Name = "type4",
                    OrganizationId = 3
                }
            };
            _eventTypesDbSet.SetDbSetDataForAsync(types.AsQueryable());
        }

        private void MockUsers()
        {
            var types = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "1"
                }
            };
            _usersDbSet.SetDbSetDataForAsync(types.AsQueryable());
        }

        private Guid[] MockEventsListTest()
        {
            var guids = Enumerable.Repeat(0, 4).Select(_ => Guid.NewGuid()).ToArray();
            var participant1 = new EventParticipant
            {
                ApplicationUserId = "responsibleUserId",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 1,
                EventId = guids[0]
            };

            var participant2 = new EventParticipant
            {
                ApplicationUserId = "testUser1",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 2,
                EventId = guids[0]
            };

            var participant3 = new EventParticipant
            {
                ApplicationUserId = "testUser2",
                Created = DateTime.UtcNow.AddDays(-2),
                Id = 3,
                EventId = guids[1]
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
                    EventOptions = new List<EventOption>
                    {
                        new EventOption
                        {
                            Id = 1,
                            Option = "Notyetedited1"
                        },
                        new EventOption
                        {
                            Id = 2,
                            Option = "Notyetedited2"
                        }
                    }
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
            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());
            return guids;
        }

        private void MockEventOptions()
        {
            var types = new List<EventOption>
            {
                new EventOption
                {
                    Id = 1,
                    Option = "test1"
                },
                new EventOption
                {
                    Id = 2,
                    Option = "test2"
                }
            };
            _eventOptionsDbSet.SetDbSetDataForAsync(types.AsQueryable());
        }

        private void MockPermissionService(IPermissionService permissionService)
        {
            permissionService.UserHasPermissionAsync(Arg.Is<UserAndOrganizationDto>(x => x.UserId == "1"), AdministrationPermissions.Event).Returns(true);
            permissionService.UserHasPermissionAsync(Arg.Is<UserAndOrganizationDto>(x => x.UserId == "2"), AdministrationPermissions.Event).Returns(false);
        }

        private Guid MockEventDelete()
        {
            var eventId = Guid.NewGuid();
            var events = new List<Event>
            {
                new Event
                {
                    Id = eventId,
                    Name = "event name",
                    Description = "event desc",
                    ImageName = "event image",
                    MaxParticipants = 10,
                    ResponsibleUserId = "eventHostId",
                    OrganizationId = 2
                }
            };

            _eventOptionsDbSet.SetDbSetDataForAsync(new List<EventOption>().AsQueryable());
            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());
            return eventId;
        }

        private Guid MockEventWithAllChildEntities()
        {
            var eventId = Guid.NewGuid();
            var responsibleUser1 = new ApplicationUser
            {
                Id = "responsibleUser1",
                FirstName = "user1f",
                LastName = "user1l",
                PictureId = "test picture"
            };

            var userParticipant1 = new ApplicationUser
            {
                Id = "userParticipant1",
                FirstName = "user3f",
                LastName = "user3l",
                PictureId = "test picture"
            };

            var userParticipant2 = new ApplicationUser
            {
                Id = "userParticipant2",
                FirstName = "user4f",
                LastName = "user4l",
                PictureId = "test picture"
            };

            var eventParticipants = new List<EventParticipant>
            {
                new EventParticipant
                {
                    Id = 1,
                    ApplicationUser = userParticipant1,
                    ApplicationUserId = "userParticipant1",
                    EventId = eventId
                },
                new EventParticipant
                {
                    Id = 2,
                    ApplicationUser = userParticipant2,
                    ApplicationUserId = "userParticipant2",
                    EventId = eventId
                }
            };

            var eventOptions = new List<EventOption>
            {
                new EventOption
                {
                    Id = 1,
                    Option = "Option1",
                    EventParticipants = eventParticipants
                },
                new EventOption
                {
                    Id = 2,
                    Option = "Option2",
                    EventParticipants = new List<EventParticipant>()
                }
            };

            var eventTypes = new List<EventType>
            {
                new EventType
                {
                    Id = 1,
                    Name = "type1",
                    OrganizationId = 2
                },
                new EventType
                {
                    Id = 2,
                    Name = "type2",
                    OrganizationId = 2
                },
                new EventType
                {
                    Id = 3,
                    Name = "type3",
                    OrganizationId = 2
                }
            };

            var events = new List<Event>
            {
                new Event
                {
                    Id = eventId,
                    Description = "desc",
                    EventOptions = eventOptions,
                    EventParticipants = eventParticipants,
                    Offices = "[\"1\"]",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    RegistrationDeadline = DateTime.UtcNow,
                    MaxChoices = 1,
                    MaxParticipants = 3,
                    Name = "name",
                    OrganizationId = 2,
                    ResponsibleUser = responsibleUser1,
                    ResponsibleUserId = responsibleUser1.Id,
                    ImageName = "image",
                    Place = "place",
                    EventType = eventTypes.Last(),
                    EventTypeId = 3
                }
            };

            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());

            var offices = new List<Office>
            {
                new Office
                {
                    Id = 1,
                    Address = new Address { City = "Vilnius", Building = "Ofisas", Country = "Lithuania", Street = "Lvovo" },
                    Name = "office1",
                    OrganizationId = 1
                }
            };

            _officeDbSet.SetDbSetDataForAsync(offices.AsQueryable());

            return eventId;
        }
    }
}
