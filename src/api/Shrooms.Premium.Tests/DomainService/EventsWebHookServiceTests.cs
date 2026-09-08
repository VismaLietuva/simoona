using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.Wall;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Events;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    [TestFixture]
    public class EventsWebHookServiceTests
    {
        private IUnitOfWork2 _uow;
        private DbSet<Event> _eventsDbSet;
        private DbSet<EventOption> _optionsDbSet;
        private DbSet<EventQuestion> _questionsDbSet;
        private IEventsWebHookService _service;
        private Event _expiredEvent;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _eventsDbSet = _uow.MockDbSetForAsync(new List<Event>());
            _optionsDbSet = _uow.MockDbSetForAsync(new List<EventOption>());
            _questionsDbSet = _uow.MockDbSetForAsync(new List<EventQuestion>());

            var systemClock = Substitute.For<ISystemClock>();
            systemClock.UtcNow.Returns(new DateTime(2026, 9, 1));

            var wallService = Substitute.For<IWallService>();
            wallService.CreateNewWallAsync(Arg.Any<CreateWallDto>()).Returns(77);

            var appSettings = Substitute.For<IApplicationSettings>();

            _service = new EventsWebHookService(_uow, systemClock, wallService, appSettings);
        }

        [Test]
        public async Task Should_Clone_The_Question_Tree_Onto_The_Next_Occurrence()
        {
            MockExpiredRecurringEventWithQuestions();

            await _service.UpdateRecurringEventsAsync();

            var clonedQuestions = _questionsDbSet.ReceivedCalls()
                .Where(call => call.GetMethodInfo().Name == nameof(DbSet<EventQuestion>.Add))
                .Select(call => (EventQuestion)call.GetArguments()[0])
                .ToList();

            Assert.That(clonedQuestions, Has.Count.EqualTo(2), "both questions must reach the new occurrence");

            var dish = clonedQuestions.Single(q => q.Title == "Pick your dish");
            Assert.That(dish.Options.Select(o => o.Option), Is.EquivalentTo(new[] { "Pasta", "Pizza" }));
            Assert.That(dish.Options.Select(o => o.Order), Is.EquivalentTo(new[] { 0, 1 }), "Order must survive the clone");

            var pizza = clonedQuestions.Single(q => q.Title == "Which pizza?");
            Assert.That(pizza.ShowIfOption, Is.Not.Null, "the condition must be rewired to the cloned trigger");
            Assert.That(pizza.ShowIfOption.Option, Is.EqualTo("Pizza"));
            Assert.That(pizza.ShowIfOptionId, Is.Null, "the cloned trigger has no ID yet, so the link rides the navigation");
        }

        [Test]
        public async Task Should_Not_Clone_Question_Options_As_Loose_Legacy_Options()
        {
            MockExpiredRecurringEventWithQuestions();

            await _service.UpdateRecurringEventsAsync();

            var looseOptions = _optionsDbSet.ReceivedCalls()
                .Where(call => call.GetMethodInfo().Name == nameof(DbSet<EventOption>.Add))
                .Select(call => (EventOption)call.GetArguments()[0])
                .ToList();

            Assert.That(looseOptions.Select(o => o.Option), Is.EquivalentTo(new[] { "Soup" }),
                "only the legacy option may be cloned loose; answers cloned this way resurface as food choices");
            Assert.That(looseOptions.Single().Rule, Is.EqualTo(OptionRules.IgnoreSingleJoin),
                "Rule must survive the clone");
        }

        [Test]
        public async Task Should_Catch_Up_To_A_Future_Occurrence_When_Runs_Were_Missed()
        {
            MockExpiredRecurringEventWithQuestions();

            await _service.UpdateRecurringEventsAsync();

            var newEvent = GetClonedEvent();

            // The source ended 2026-08-01 and the clock reads 2026-09-01, so five weekly shifts.
            Assert.That(newEvent.LocalStartDate, Is.EqualTo(new DateTime(2026, 9, 4)));
            Assert.That(newEvent.LocalEndDate, Is.EqualTo(new DateTime(2026, 9, 5)));
            Assert.That(newEvent.LocalRegistrationDeadline, Is.EqualTo(new DateTime(2026, 9, 3)),
                "the registration lead time must survive the catch-up");
            Assert.That(newEvent.EndDate, Is.GreaterThan(new DateTime(2026, 9, 1)),
                "a single run must land the occurrence in the future, not one period on from a stale one");
        }

        [Test]
        public async Task Should_Carry_The_Rsvp_And_Widget_Flags_But_Not_The_Pin()
        {
            MockExpiredRecurringEventWithQuestions();

            await _service.UpdateRecurringEventsAsync();

            var newEvent = GetClonedEvent();

            Assert.That(newEvent.AllowMaybeGoing, Is.True);
            Assert.That(newEvent.AllowNotGoing, Is.True);
            Assert.That(newEvent.IsShownInUpcomingEventsWidget, Is.True);
            Assert.That(newEvent.MaxParticipants, Is.EqualTo(12));
            Assert.That(newEvent.IsPinned, Is.False, "a pin curates one occurrence, not the series");
        }

        [Test]
        public async Task Should_Not_Carry_Attendees_Onto_The_Next_Occurrence()
        {
            MockExpiredRecurringEventWithQuestions();

            await _service.UpdateRecurringEventsAsync();

            Assert.That(GetClonedEvent().EventParticipants, Is.Null.Or.Empty,
                "attendance is per occurrence; everyone registers again for the next one");
            Assert.That(_expiredEvent.EventParticipants, Has.Count.EqualTo(2),
                "the finished occurrence keeps its own attendance record");
        }

        private Event GetClonedEvent()
        {
            return _eventsDbSet.ReceivedCalls()
                .Where(call => call.GetMethodInfo().Name == nameof(DbSet<Event>.Add))
                .Select(call => (Event)call.GetArguments()[0])
                .Single();
        }

        private void MockExpiredRecurringEventWithQuestions()
        {
            var eventId = Guid.NewGuid();

            var soup = new EventOption { Id = 10, EventId = eventId, Option = "Soup", QuestionId = null, Order = 3, Rule = OptionRules.IgnoreSingleJoin };
            var pasta = new EventOption { Id = 90, EventId = eventId, Option = "Pasta", QuestionId = 5, Order = 0 };
            var pizzaOption = new EventOption { Id = 91, EventId = eventId, Option = "Pizza", QuestionId = 5, Order = 1 };
            var margherita = new EventOption { Id = 92, EventId = eventId, Option = "Margherita", QuestionId = 6, Order = 0 };

            var events = new List<Event>
            {
                new Event
                {
                    Id = eventId,
                    OrganizationId = 2,
                    Name = "Weekly lunch",
                    EventRecurring = EventRecurrenceOptions.EveryWeek,
                    EndDate = new DateTime(2026, 8, 1),
                    LocalStartDate = new DateTime(2026, 7, 31),
                    LocalEndDate = new DateTime(2026, 8, 1),
                    LocalRegistrationDeadline = new DateTime(2026, 7, 30),
                    ResponsibleUserId = "host1",
                    ResponsibleUser = new ApplicationUser { Id = "host1", TimeZone = DataLayerConstants.DefaultTimeZone },
                    MaxChoices = 1,
                    MaxParticipants = 12,
                    AllowMaybeGoing = true,
                    AllowNotGoing = true,
                    IsShownInUpcomingEventsWidget = true,
                    IsPinned = true,
                    EventParticipants = new List<EventParticipant>
                    {
                        new EventParticipant { EventId = eventId, ApplicationUserId = "guest1", AttendStatus = 1 },
                        new EventParticipant { EventId = eventId, ApplicationUserId = "guest2", AttendStatus = 1 }
                    },
                    EventOptions = new List<EventOption> { soup, pasta, pizzaOption, margherita },
                    EventQuestions = new List<EventQuestion>
                    {
                        new EventQuestion
                        {
                            Id = 5, EventId = eventId, Title = "Pick your dish", Order = 0,
                            SelectType = EventQuestionSelectType.Single, IsRequired = true,
                            ShowIfOptionId = null, Options = new List<EventOption> { pasta, pizzaOption }
                        },
                        new EventQuestion
                        {
                            Id = 6, EventId = eventId, Title = "Which pizza?", Order = 1,
                            SelectType = EventQuestionSelectType.Single, IsRequired = true,
                            ShowIfOptionId = 91, Options = new List<EventOption> { margherita }
                        }
                    }
                }
            };

            _expiredEvent = events.Single();
            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());
        }
    }
}
