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
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.FilterPresets;
using Shrooms.Premium.Domain.Services.Events.Utilities;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    public class EventUtilitiesServiceTests
    {
        private IEventUtilitiesService _eventUtilitiesService;

        private IFilterPresetService _filterPresetService;

        private DbSet<EventType> _eventTypesDbSet;
        private DbSet<EventOption> _eventOptionsDbSet;
        private DbSet<Event> _eventDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _filterPresetService = Substitute.For<IFilterPresetService>();

            _eventOptionsDbSet = uow.MockDbSetForAsync<EventOption>();
            _eventTypesDbSet = uow.MockDbSetForAsync<EventType>();
            _eventDbSet = uow.MockDbSetForAsync<Event>(new List<Event>());

            _eventUtilitiesService = new EventUtilitiesService(uow, _filterPresetService);
        }

        [Test]
        public async Task Should_Delete_Event_Options()
        {
            var eventId = MockCommentsForEvent();
            await _eventUtilitiesService.DeleteEventOptionsAsync(eventId, "testUserId");
            _eventOptionsDbSet.Received(3).Remove(Arg.Any<EventOption>());
        }

        [Test]
        public async Task Should_Return_Correctly_Mapped_Event_Types()
        {
            MockEventTypes();
            const int organizationId = 2;

            var result = (await _eventUtilitiesService.GetEventTypesAsync(organizationId)).ToList();

            ClassicAssert.AreEqual(result.Count, 3);
            ClassicAssert.AreEqual(result.First(x => x.Id == 1).Name, "type1");
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
            ClassicAssert.AreEqual("Option1", options.ToArray()[0].Option);
            ClassicAssert.AreEqual("Option2", options.ToArray()[1].Option);
            ClassicAssert.AreEqual(2, options.ToArray()[0].Count);
            ClassicAssert.AreEqual(1, options.ToArray()[1].Count);
            ClassicAssert.AreEqual(2, options.Count);
        }

        [Test]
        public async Task Should_Return_Chosen_Options_From_Sign_Up_Questions()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var guid = MockChosenOptionsWithQuestionsForExport();

            var options = (await _eventUtilitiesService.GetEventChosenOptionsAsync(guid, userAndOrg)).ToList();

            ClassicAssert.AreEqual(3, options.Count);

            ClassicAssert.AreEqual(null, options[0].Question);
            ClassicAssert.AreEqual(null, options[0].QuestionId);
            ClassicAssert.AreEqual("Pizza", options[0].Option);
            ClassicAssert.AreEqual(2, options[0].Count);

            ClassicAssert.AreEqual("Dietary needs", options[1].Question);
            ClassicAssert.AreEqual(7, options[1].QuestionId);
            ClassicAssert.AreEqual("Vegan", options[1].Option);
            ClassicAssert.AreEqual(1, options[1].Count);

            ClassicAssert.AreEqual("T-shirt size", options[2].Question);
            ClassicAssert.AreEqual(9, options[2].QuestionId);
            ClassicAssert.AreEqual("M", options[2].Option);
            ClassicAssert.AreEqual(1, options[2].Count);
        }

        [Test]
        public async Task Should_Order_Flat_Options_By_Id_When_They_Share_An_Order()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };
            var guid = MockFlatOptionsSharingAnOrderForExport();

            var options = (await _eventUtilitiesService.GetEventChosenOptionsAsync(guid, userAndOrg)).ToList();

            ClassicAssert.AreEqual(new[] { "First", "Second", "Third" }, options.Select(option => option.Option).ToArray());
        }

        [Test]
        public async Task Should_Return_Event_Type_With_Active_Event()
        {
            // Arrange
            MockEventTypes();

            // Act
            var eventType = await _eventUtilitiesService.GetEventTypeAsync(3, 4);

            // Assert
            ClassicAssert.AreEqual(true, eventType.HasActiveEvents);
            ClassicAssert.AreEqual("type4", eventType.Name);
        }

        [Test]
        public async Task Should_Return_Event_Type_With_Inactive_Event()
        {
            // Arrange
            MockEventTypes();

            // Act
            var eventType = await _eventUtilitiesService.GetEventTypeAsync(2, 3);

            // Assert
            ClassicAssert.AreEqual(false, eventType.HasActiveEvents);
            ClassicAssert.AreEqual("type3", eventType.Name);
        }

        [TestCase(2, 1)]
        [TestCase(2111, 0)]
        public async Task GetEventTypesToRemind_DifferentOrganizations_ReturnsCorrectAmountEventTypes(int orgId, int amount)
        {
            MockEventTypes();

            var eventTypes = (await _eventUtilitiesService.GetEventTypesToRemindAsync(orgId)).ToList();

            ClassicAssert.AreEqual(amount, eventTypes.Count);
        }

        [Test]
        public async Task GetEventTypesToRemind_OrganizationIdFour_ReturnsCorrectEventType()
        {
            MockEventTypes();

            var eventTypes = (await _eventUtilitiesService.GetEventTypesToRemindAsync(4)).ToList();

            ClassicAssert.AreEqual(1, eventTypes.Count);
            ClassicAssert.AreEqual(5, eventTypes.First().Id);
            ClassicAssert.AreEqual("type5", eventTypes.First().Name);
        }

        [Test]
        public async Task DeleteEventType_WhenValid_RemovesTypeFromPresets()
        {
            // Arrange
            const int id = 1;

            var userOrg = new UserAndOrganizationDto
            {
                UserId = "Id",
                OrganizationId = 2
            };

            MockEventTypes();

            // Act
            await _eventUtilitiesService.DeleteEventTypeAsync(id, userOrg);

            // Assert
            await _filterPresetService.Received(1)
                .RemoveDeletedTypeFromPresetsAsync(Arg.Is(id.ToString()), Arg.Is(FilterType.Events), Arg.Is(userOrg.OrganizationId));
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

        private Guid MockChosenOptionsWithQuestionsForExport()
        {
            var eventId = Guid.NewGuid();

            var @event = new Event
            {
                Id = eventId,
                OrganizationId = 2,
                ResponsibleUserId = "user"
            };

            var dietaryNeeds = new EventQuestion
            {
                Id = 7,
                EventId = eventId,
                Event = @event,
                Title = "Dietary needs",
                Order = 0
            };

            var tShirtSize = new EventQuestion
            {
                Id = 9,
                EventId = eventId,
                Event = @event,
                Title = "T-shirt size",
                Order = 1
            };

            // Scrambled on purpose — the service is what has to order these.
            var options = new List<EventOption>
            {
                new EventOption
                {
                    EventId = eventId,
                    Event = @event,
                    Option = "M",
                    Order = 0,
                    QuestionId = tShirtSize.Id,
                    Question = tShirtSize,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant { EventId = eventId }
                    }
                },
                new EventOption
                {
                    EventId = eventId,
                    Event = @event,
                    Option = "Vegan",
                    Order = 0,
                    QuestionId = dietaryNeeds.Id,
                    Question = dietaryNeeds,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant { EventId = eventId }
                    }
                },
                new EventOption
                {
                    EventId = eventId,
                    Event = @event,
                    Option = "Pizza",
                    Order = 0,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant { EventId = eventId },
                        new EventParticipant { EventId = eventId }
                    }
                },
                new EventOption
                {
                    EventId = eventId,
                    Event = @event,
                    Option = "Vegetarian",
                    Order = 1,
                    QuestionId = dietaryNeeds.Id,
                    Question = dietaryNeeds,
                    EventParticipants = new List<EventParticipant>()
                }
            };

            _eventDbSet.SetDbSetDataForAsync(new List<Event> { @event }.AsQueryable());
            _eventOptionsDbSet.SetDbSetDataForAsync(options.AsQueryable());
            return eventId;
        }

        private Guid MockFlatOptionsSharingAnOrderForExport()
        {
            var eventId = Guid.NewGuid();

            var @event = new Event
            {
                Id = eventId,
                OrganizationId = 2,
                ResponsibleUserId = "user"
            };

            // Every legacy flat option is written with Order 0, so Id is the only thing separating
            // them. Seeded newest-first to prove the service does not just echo insertion order.
            var options = new[] { 30, 20, 10 }
                .Zip(new[] { "Third", "Second", "First" }, (id, name) => new EventOption
                {
                    Id = id,
                    EventId = eventId,
                    Event = @event,
                    Option = name,
                    Order = 0,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant { EventId = eventId }
                    }
                })
                .ToList();

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
