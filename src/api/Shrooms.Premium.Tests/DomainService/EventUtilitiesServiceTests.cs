using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.FilterPresets;
using Shrooms.Premium.Domain.Services.Events.Utilities;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    public class EventUtilitiesServiceTests
    {
        private IEventUtilitiesService _eventUtilitiesService;
        private DbSet<EventType> _eventTypesDbSet;
        private DbSet<EventOption> _eventOptionsDbSet;
        private DbSet<Event> _eventDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            var filterPresetService = Substitute.For<IFilterPresetService>();

            _eventOptionsDbSet = uow.MockDbSetForAsync<EventOption>();
            _eventTypesDbSet = uow.MockDbSetForAsync<EventType>();
            _eventDbSet = uow.MockDbSetForAsync<Event>();

            _eventUtilitiesService = new EventUtilitiesService(uow, filterPresetService);
        }

        [Test]
        public async Task Should_Delete_Event_Options()
        {
            var eventId = MockCommentsForEvent();
            await _eventUtilitiesService.DeleteByEventAsync(eventId, "testUserId");
            _eventOptionsDbSet.Received(3).Remove(Arg.Any<EventOption>());
        }

        [Test]
        public async Task Should_Return_Correctly_Mapped_Event_Types()
        {
            MockEventTypes();
            const int organizationId = 2;

            var result = (await _eventUtilitiesService.GetEventTypesAsync(organizationId)).ToList();

            Assert.AreEqual(result.Count, 3);
            Assert.AreEqual(result.First(x => x.Id == 1).Name, "type1");
        }

        [Test]
        public async Task Should_Return_Event_Chosen_Options()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var guid = MockParticipantsWithOptionsForExport();

            var options = (await _eventUtilitiesService.GetEventChosenOptionsAsync(guid, userAndOrg)).ToList();
            Assert.AreEqual("Option1", options.ToArray()[0].Option);
            Assert.AreEqual("Option2", options.ToArray()[1].Option);
            Assert.AreEqual(2, options.ToArray()[0].Count);
            Assert.AreEqual(1, options.ToArray()[1].Count);
            Assert.AreEqual(2, options.Count);
        }

        [Test]
        public async Task Should_Return_Event_Type_With_Active_Event()
        {
            // Arrange
            MockEventTypes();

            // Act
            var eventType = await _eventUtilitiesService.GetEventTypeAsync(3, 4);

            // Assert
            Assert.AreEqual(true, eventType.HasActiveEvents);
            Assert.AreEqual("type4", eventType.Name);
        }

        [Test]
        public async Task Should_Return_Event_Type_With_Inactive_Event()
        {
            // Arrange
            MockEventTypes();

            // Act
            var eventType = await _eventUtilitiesService.GetEventTypeAsync(2, 3);

            // Assert
            Assert.AreEqual(false, eventType.HasActiveEvents);
            Assert.AreEqual("type3", eventType.Name);
        }

        [TestCase(2, 1)]
        [TestCase(2111, 0)]
        public async Task GetEventTypesToRemind_DifferentOrganizations_ReturnsCorrectAmountEventTypes(int orgId, int amount)
        {
            MockEventTypes();

            var eventTypes = (await _eventUtilitiesService.GetEventTypesToRemindAsync(orgId)).ToList();

            Assert.AreEqual(amount, eventTypes.Count);
        }

        [Test]
        public async Task GetEventTypesToRemind_OrganizationIdFour_ReturnsCorrectEventType()
        {
            MockEventTypes();

            var eventTypes = (await _eventUtilitiesService.GetEventTypesToRemindAsync(4)).ToList();

            Assert.AreEqual(1, eventTypes.Count);
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
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant
                        {
                            EventId = eventId
                        },
                        new EventParticipant
                        {
                            EventId = eventId
                        },
                        new EventParticipant
                        {
                            EventId = default
                        }
                    }
                },
                new EventOption
                {
                    EventId = eventId,
                    Option = "Option2",
                    Event = @event,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant
                        {
                            EventId = eventId
                        },
                        new EventParticipant
                        {
                            EventId = default
                        }
                    }
                },
                new EventOption
                {
                    EventId = eventId,
                    Option = "Option3",
                    Event = @event,
                    EventParticipants = new List<EventParticipant>()
                }
            };
            _eventDbSet.SetDbSetDataForAsync(new List<Event> { @event }.AsQueryable());
            _eventOptionsDbSet.SetDbSetDataForAsync(options.AsQueryable());
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
            _eventOptionsDbSet.SetDbSetDataForAsync(options.AsQueryable());
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
                    OrganizationId = 2
                },
                new EventType
                {
                    Id = 3,
                    Name = "type3",
                    OrganizationId = 2,
                    Events = new List<Event>
                    {
                        new Event
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
                    Events = new List<Event>
                    {
                        new Event
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
            _eventTypesDbSet.SetDbSetDataForAsync(types.AsQueryable());
        }
    }
}
