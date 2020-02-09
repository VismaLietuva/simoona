﻿using NSubstitute;
using NUnit.Framework;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Domain.Services.Events.Utilities;
using Shrooms.EntityModels.Models.Events;
using Shrooms.UnitTests.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;

namespace Shrooms.UnitTests.DomainService
{
    public class EventUtilitiesServiceTests
    {
        private IEventUtilitiesService _eventUtilitiesService;
        private IDbSet<EventType> _eventTypesDbSet;
        private IDbSet<EventOption> _eventOptionsDbSet;
        private IDbSet<Event> _eventDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _eventOptionsDbSet = uow.MockDbSet<EventOption>();
            _eventTypesDbSet = uow.MockDbSet<EventType>();
            _eventDbSet = uow.MockDbSet<Event>();

            _eventUtilitiesService = new EventUtilitiesService(uow);
        }

        [Test]
        public void Should_Delete_Event_Options()
        {
            var eventId = MockCommentsForEvent();
            _eventUtilitiesService.DeleteByEvent(eventId, "testUserId");
            _eventOptionsDbSet.Received(3).Remove(Arg.Any<EventOption>());
        }

        [Test]
        public void Should_Return_Correctly_Mapped_Event_Types()
        {
            MockEventTypes();
            var organizationId = 2;
            var result = _eventUtilitiesService.GetEventTypes(organizationId);
            Assert.AreEqual(result.Count(), 3);
            Assert.AreEqual(result.First(x => x.Id == 1).Name, "type1");
        }

        [Test]
        public void Should_Return_Event_Chosen_Options()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
            };
            var guid = MockParticipantsWithOptionsForExport();

            var options = _eventUtilitiesService.GetEventChosenOptions(guid, userAndOrg);
            Assert.AreEqual("Option1", options.ToArray()[0].Option);
            Assert.AreEqual("Option2", options.ToArray()[1].Option);
            Assert.AreEqual(2, options.ToArray()[0].Count);
            Assert.AreEqual(1, options.ToArray()[1].Count);
            Assert.AreEqual(2, options.Count());
        }

        [Test]
        public void Should_Return_Event_Type_With_Active_Event()
        {
            //Arange
            MockEventTypes();

            //Act
            var eventType = _eventUtilitiesService.GetEventType(3, 4);

            //Assert
            Assert.AreEqual(true, eventType.HasActiveEvents);
            Assert.AreEqual("type4", eventType.Name);
        }

        [Test]
        public void Should_Return_Event_Type_With_Inactive_Event()
        {
            //Arange
            MockEventTypes();

            //Act
            var eventType = _eventUtilitiesService.GetEventType(2, 3);

            //Assert
            Assert.AreEqual(false, eventType.HasActiveEvents);
            Assert.AreEqual("type3", eventType.Name);
        }

        [TestCase(2, 1)]
        [TestCase(2111, 0)]
        public void GetEventTypesToRemind_DifferentOrganizations_ReturnsCorrectAmountEventTypes(int orgId, int amount)
        {
            MockEventTypes();

            var eventTypes = _eventUtilitiesService.GetEventTypesToRemind(orgId);

            Assert.AreEqual(amount, eventTypes.Count());
        }

        [Test]
        public void GetEventTypesToRemind_OrganizationIdFour_ReturnsCorrectEventType()
        {
            MockEventTypes();

            var eventTypes = _eventUtilitiesService.GetEventTypesToRemind(4);

            Assert.AreEqual(1, eventTypes.Count());
            Assert.AreEqual(5, eventTypes.First().Id);
            Assert.AreEqual("type5", eventTypes.First().Name);
        }

        private Guid MockParticipantsWithOptionsForExport()
        {
            var eventId = Guid.NewGuid();

            var @event = new Event
            {
                Id = eventId,
                OrganizationId = 2,
                ResponsibleUserId = "user"
            };

            var options = new List<EventOption>
            {
                new EventOption
                {
                    EventId = eventId,
                    Option = "Option1",
                    Event = @event,
                    EventParticipants = new List<EventParticipant>()
                    {
                        new EventParticipant()
                        {
                            EventId = eventId
                        },
                        new EventParticipant()
                        {
                            EventId = eventId
                        },
                        new EventParticipant()
                        {
                            EventId = default(Guid)
                        }
                    },
                },
                new EventOption
                {
                    EventId = eventId,
                    Option = "Option2",
                    Event = @event,
                    EventParticipants = new List<EventParticipant>()
                    {
                        new EventParticipant()
                        {
                            EventId = eventId
                        },
                        new EventParticipant()
                        {
                            EventId = default(Guid)
                        }
                    }
                },
                new EventOption
                {
                    EventId = eventId,
                    Option = "Option3",
                    Event = @event,
                    EventParticipants = new List<EventParticipant>()
                },
            };
            _eventDbSet.SetDbSetData(new List<Event> { @event }.AsQueryable());
            _eventOptionsDbSet.SetDbSetData(options.AsQueryable());
            return eventId;
        }

        private Guid MockCommentsForEvent()
        {
            var eventId = Guid.NewGuid();
            var options = new List<EventOption>
            {
                new EventOption
                {
                    EventId = eventId,
                    Option = "Option1"
                },
                new EventOption
                {
                    EventId = eventId,
                    Option = "Option2"
                },
                new EventOption
                {
                    EventId = eventId,
                    Option = "Option3"
                },
                new EventOption
                {
                    EventId = Guid.NewGuid(),
                    Option = "Option4"
                }
            };
            _eventOptionsDbSet.SetDbSetData(options.AsQueryable());
            return eventId;
        }

        private void MockEventTypes()
        {
            var types = new List<EventType>
            {
                new EventType
                {
                    Id = 1,
                    Name = "type1",
                    OrganizationId = 2,
                    SendWeeklyReminders = true
                },
                new EventType
                {
                    Id = 2,
                    Name = "type2",
                    OrganizationId = 2,
                },
                new EventType
                {
                    Id = 3,
                    Name = "type3",
                    OrganizationId = 2,
                    Events = new List<Event>()
                    {
                        new Event()
                        {
                            EndDate = DateTime.UtcNow.AddHours(-1)
                        }
                    }
                },
                new EventType
                {
                    Id = 4,
                    Name = "type4",
                    OrganizationId = 3,
                    Events = new List<Event>()
                    {
                        new Event()
                        {
                            EndDate = DateTime.UtcNow.AddHours(1)
                        }
                    }
                },
                new EventType
                {
                    Id = 5,
                    Name = "type5",
                    OrganizationId = 4,
                    SendWeeklyReminders = true
                }
            };
            _eventTypesDbSet.SetDbSetData(types.AsQueryable());
        }
    }
}