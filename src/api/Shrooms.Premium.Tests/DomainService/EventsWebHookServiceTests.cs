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

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _eventsDbSet = _uow.MockDbSetForAsync(new List<Event>());
            _optionsDbSet = _uow.MockDbSetForAsync(new List<EventOption>());
            _questionsDbSet = _uow.MockDbSetForAsync(new List<EventQuestion>());

            var systemClock = Substitute.For<ISystemClock>();
            systemClock.UtcNow.Returns(DateTime.Parse("2026-09-01"));

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
                    EndDate = DateTime.Parse("2026-08-01"),
                    LocalStartDate = DateTime.Parse("2026-07-31"),
                    LocalEndDate = DateTime.Parse("2026-08-01"),
                    LocalRegistrationDeadline = DateTime.Parse("2026-07-30"),
                    ResponsibleUserId = "host1",
                    ResponsibleUser = new ApplicationUser { Id = "host1", TimeZone = DataLayerConstants.DefaultTimeZone },
                    MaxChoices = 1,
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

            _eventsDbSet.SetDbSetDataForAsync(events.AsQueryable());
        }
    }
}
