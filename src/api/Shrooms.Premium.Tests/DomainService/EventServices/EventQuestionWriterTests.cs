using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;
using Shrooms.Premium.Domain.Services.Events;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    public class EventQuestionWriterTests
    {
        private IUnitOfWork2 _uow;
        private IEventQuestionWriter _writer;
        private DbSet<EventQuestion> _questionsDbSet;
        private DbSet<EventOption> _optionsDbSet;

        private readonly Guid _eventId = Guid.NewGuid();

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _questionsDbSet = _uow.MockDbSetForAsync(new List<EventQuestion>());
            _optionsDbSet = _uow.MockDbSetForAsync(new List<EventOption>());

            _writer = new EventQuestionWriter(_uow, new EventQuestionStructureValidator());
        }

        private static EventQuestionStructureDto Question(
            string clientId,
            int order,
            string title,
            params EventQuestionOptionStructureDto[] options)
        {
            return new EventQuestionStructureDto
            {
                Id = null,
                ClientId = clientId,
                Title = title,
                Order = order,
                SelectType = EventQuestionSelectType.Single,
                IsRequired = true,
                Options = options.ToList()
            };
        }

        private static EventQuestionOptionStructureDto Option(string clientId, string name, int order)
        {
            return new EventQuestionOptionStructureDto
            {
                Id = null,
                ClientId = clientId,
                Name = name,
                Order = order,
                Rule = OptionRules.Default
            };
        }

        [Test]
        public async Task Should_Insert_A_Question_With_Its_Options()
        {
            var questions = new List<EventQuestionStructureDto>
            {
                Question("q1", 0, "Pick your dish", Option("o1", "Pizza", 0), Option("o2", "Pasta", 1))
            };

            await _writer.WriteAsync(_eventId, questions, "user-1");

            _questionsDbSet.Received(1).Add(Arg.Is<EventQuestion>(q => q.Title == "Pick your dish" && q.EventId == _eventId));
            _optionsDbSet.Received(2).Add(Arg.Any<EventOption>());
            await _uow.Received(1).SaveChangesAsync("user-1");
        }

        [Test]
        public async Task Should_Wire_A_ClientId_Condition_Through_The_Navigation_Property()
        {
            var trigger = Option("o1", "Pizza", 0);
            var conditional = Question("q2", 1, "Which pizza?", Option("o3", "Margherita", 0));
            conditional.ShowIfOptionClientId = "o1";

            var questions = new List<EventQuestionStructureDto>
            {
                Question("q1", 0, "Pick your dish", trigger),
                conditional
            };

            EventQuestion captured = null;
            _questionsDbSet
                .When(x => x.Add(Arg.Is<EventQuestion>(q => q.Title == "Which pizza?")))
                .Do(call => captured = call.Arg<EventQuestion>());

            await _writer.WriteAsync(_eventId, questions, "user-1");

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured.ShowIfOption, Is.Not.Null, "the trigger row has no ID yet, so the navigation property must carry the link");
            Assert.That(captured.ShowIfOption.Option, Is.EqualTo("Pizza"));
        }

        [Test]
        public void Should_Reject_A_Condition_Whose_Trigger_Is_Absent_From_The_Payload()
        {
            var conditional = Question("q1", 0, "Which pizza?", Option("o1", "Margherita", 0));
            conditional.ShowIfOptionClientId = "removed-in-this-request";

            var questions = new List<EventQuestionStructureDto> { conditional };

            Assert.ThrowsAsync<EventException>(async () => await _writer.WriteAsync(_eventId, questions, "user-1"));
        }

        [Test]
        public void Should_Not_Write_Anything_When_Validation_Fails()
        {
            // A condition pointing at a question with a HIGHER order violates the strictly-lower
            // invariant. Nothing may reach the database — the spec forbids persisting a tree whose
            // condition was silently dropped.
            var early = Question("q1", 0, "Early", Option("o1", "A", 0));
            early.ShowIfOptionClientId = "o2";              // owned by q2, which sits at a higher order
            var late = Question("q2", 1, "Late", Option("o2", "B", 0));

            var questions = new List<EventQuestionStructureDto> { early, late };

            Assert.ThrowsAsync<EventException>(async () => await _writer.WriteAsync(_eventId, questions, "user-1"));

            _questionsDbSet.DidNotReceive().Add(Arg.Any<EventQuestion>());
            _optionsDbSet.DidNotReceive().Add(Arg.Any<EventOption>());
            Assert.ThrowsAsync<EventException>(async () => await _writer.WriteAsync(_eventId, questions, "user-1"));
        }

        [Test]
        public void Should_Not_Save_When_Validation_Fails()
        {
            var early = Question("q1", 0, "Early", Option("o1", "A", 0));
            early.ShowIfOptionClientId = "o2";
            var late = Question("q2", 1, "Late", Option("o2", "B", 0));

            Assert.ThrowsAsync<EventException>(
                async () => await _writer.WriteAsync(_eventId, new List<EventQuestionStructureDto> { early, late }, "user-1"));

            _uow.DidNotReceive().SaveChangesAsync(Arg.Any<string>());
        }

        [Test]
        public async Task Should_Soft_Delete_Questions_Absent_From_The_Payload()
        {
            var existing = new EventQuestion
            {
                Id = 7,
                EventId = _eventId,
                Title = "Old question",
                Order = 0,
                IsDeleted = false,
                Options = new List<EventOption>()
            };

            _uow = Substitute.For<IUnitOfWork2>();
            _questionsDbSet = _uow.MockDbSetForAsync(new List<EventQuestion> { existing });
            _optionsDbSet = _uow.MockDbSetForAsync(new List<EventOption>());
            _writer = new EventQuestionWriter(_uow, new EventQuestionStructureValidator());

            await _writer.WriteAsync(_eventId, new List<EventQuestionStructureDto>(), "user-1");

            Assert.That(existing.IsDeleted, Is.True);
            _questionsDbSet.DidNotReceive().Remove(Arg.Any<EventQuestion>());
        }

        [Test]
        public async Task Should_Save_Exactly_Once_On_A_Successful_Write()
        {
            var questions = new List<EventQuestionStructureDto>
            {
                Question("q1", 0, "Pick your dish", Option("o1", "Pizza", 0))
            };

            await _writer.WriteAsync(_eventId, questions, "user-1");

            await _uow.Received(1).SaveChangesAsync(Arg.Any<string>());
        }
    }
}
