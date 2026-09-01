using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    public class EventQuestionStructureValidatorTests
    {
        private EventQuestionStructureValidator _validator;

        [SetUp]
        public void TestInitializer()
        {
            _validator = new EventQuestionStructureValidator();
        }

        private static EventQuestionStructureDto Question(string clientId, int order, string title = "Pick your dish")
        {
            return new EventQuestionStructureDto
            {
                Id = null,
                ClientId = clientId,
                Title = title,
                Order = order,
                SelectType = EventQuestionSelectType.Single,
                IsRequired = true,
                Options = new List<EventQuestionOptionStructureDto>
                {
                    new EventQuestionOptionStructureDto { ClientId = clientId + "-o1", Name = "Pizza", Order = 0 }
                }
            };
        }

        [Test]
        public void Should_Accept_A_Valid_Flat_Payload()
        {
            var questions = new List<EventQuestionStructureDto> { Question("q1", 0) };

            Assert.DoesNotThrow(() => _validator.ValidatePayload(questions));
        }

        [Test]
        public void Should_Reject_More_Than_20_Questions()
        {
            var questions = Enumerable.Range(0, 21)
                .Select(i => Question("q" + i, i))
                .ToList();

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(questions));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionLimitExceeded));
        }

        [Test]
        public void Should_Accept_Exactly_20_Questions()
        {
            var questions = Enumerable.Range(0, 20)
                .Select(i => Question("q" + i, i))
                .ToList();

            Assert.DoesNotThrow(() => _validator.ValidatePayload(questions));
        }

        [Test]
        public void Should_Reject_More_Than_30_Options_In_One_Question()
        {
            var question = Question("q1", 0);
            question.Options = Enumerable.Range(0, 31)
                .Select(i => new EventQuestionOptionStructureDto { ClientId = "o" + i, Name = "Option", Order = i })
                .ToList();

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionOptionLimitExceeded));
        }

        [Test]
        public void Should_Reject_A_Title_Longer_Than_100_Characters()
        {
            var question = Question("q1", 0, new string('x', 101));

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionTitleInvalid));
        }

        [Test]
        public void Should_Reject_An_Empty_Title()
        {
            var question = Question("q1", 0, "   ");

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionTitleInvalid));
        }

        [Test]
        public void Should_Reject_An_Option_Name_Longer_Than_100_Characters()
        {
            var question = Question("q1", 0);
            question.Options[0].Name = new string('x', 101);

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionOptionNameInvalid));
        }

        [Test]
        public void Should_Reject_A_Condition_That_Sets_Both_OptionId_And_OptionClientId()
        {
            var question = Question("q2", 1);
            question.ShowIfOptionId = 41;
            question.ShowIfOptionClientId = "q1-o1";

            var questions = new List<EventQuestionStructureDto> { Question("q1", 0), question };

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(questions));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionConditionAmbiguous));
        }

        [Test]
        public void Should_Reject_A_ClientId_Reference_That_Matches_No_Option_In_The_Payload()
        {
            var question = Question("q2", 1);
            question.ShowIfOptionClientId = "does-not-exist";

            var questions = new List<EventQuestionStructureDto> { Question("q1", 0), question };

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(questions));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionConditionInvalid));
        }

        [Test]
        public void Should_Require_A_ClientId_When_Id_Is_Null()
        {
            var question = Question("q1", 0);
            question.ClientId = null;

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionClientIdMissing));
        }

        [Test]
        public void Should_Reject_A_Question_Whose_Options_Are_Null()
        {
            var question = Question("q1", 0);
            question.Options = null;

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionOptionsMissing));
        }

        [Test]
        public void Should_Reject_A_Question_With_No_Options()
        {
            var question = Question("q1", 0);
            question.Options = new List<EventQuestionOptionStructureDto>();

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionOptionsMissing));
        }

        [Test]
        public void Should_Reject_A_Null_Question_In_The_Payload()
        {
            var questions = new List<EventQuestionStructureDto> { Question("q1", 0), null };

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(questions));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionPayloadInvalid));
        }

        [Test]
        public void Should_Reject_A_Null_Option_In_The_Payload()
        {
            var question = Question("q1", 0);
            question.Options.Add(null);

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionPayloadInvalid));
        }

        [Test]
        public void Should_Reject_Two_Questions_Sharing_An_Id()
        {
            var first = Question("q1", 0);
            var second = Question("q2", 1);
            first.Id = 5;
            second.Id = 5;

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { first, second }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionDuplicateId));
        }

        [Test]
        public void Should_Reject_Two_Options_Sharing_An_Id_Across_Questions()
        {
            var first = Question("q1", 0);
            var second = Question("q2", 1);
            first.Options[0].Id = 100;
            second.Options[0].Id = 100;

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { first, second }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionDuplicateId));
        }

        [Test]
        public void Should_Reject_Two_Options_Sharing_A_ClientId()
        {
            var first = Question("q1", 0);
            var second = Question("q2", 1);
            second.Options[0].ClientId = first.Options[0].ClientId;

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { first, second }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionDuplicateClientId));
        }

        [Test]
        public void Should_Reject_Two_Questions_Sharing_An_Order()
        {
            var questions = new List<EventQuestionStructureDto> { Question("q1", 0), Question("q2", 0) };

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(questions));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionDuplicateOrder));
        }

        [Test]
        public void Should_Reject_Two_Options_In_One_Question_Sharing_An_Order()
        {
            var question = Question("q1", 0);
            question.Options.Add(new EventQuestionOptionStructureDto { ClientId = "q1-o2", Name = "Pasta", Order = 0 });

            var ex = Assert.Throws<EventException>(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionDuplicateOrder));
        }

        [Test]
        public void Should_Treat_A_Whitespace_Only_Trigger_ClientId_As_No_Condition()
        {
            var question = Question("q1", 0);
            question.ShowIfOptionClientId = "   ";

            Assert.DoesNotThrow(() => _validator.ValidatePayload(new List<EventQuestionStructureDto> { question }));
        }

        [Test]
        public void Should_Reject_A_Condition_Pointing_At_A_Question_With_A_Higher_Order()
        {
            var resolved = new List<ResolvedEventQuestionDto>
            {
                new ResolvedEventQuestionDto { QuestionId = 1, Order = 0, ShowIfOptionId = 20, OptionIds = new[] { 10 } },
                new ResolvedEventQuestionDto { QuestionId = 2, Order = 1, ShowIfOptionId = null, OptionIds = new[] { 20 } }
            };

            var ex = Assert.Throws<EventException>(() => _validator.ValidateResolved(resolved));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionConditionInvalid));
        }

        [Test]
        public void Should_Accept_A_Condition_Pointing_At_A_Question_With_A_Lower_Order()
        {
            var resolved = new List<ResolvedEventQuestionDto>
            {
                new ResolvedEventQuestionDto { QuestionId = 1, Order = 0, ShowIfOptionId = null, OptionIds = new[] { 10 } },
                new ResolvedEventQuestionDto { QuestionId = 2, Order = 1, ShowIfOptionId = 10, OptionIds = new[] { 20 } }
            };

            Assert.DoesNotThrow(() => _validator.ValidateResolved(resolved));
        }

        [Test]
        public void Should_Accept_A_Conditional_Chain_Exactly_5_Deep()
        {
            var resolved = BuildChain(5);

            Assert.DoesNotThrow(() => _validator.ValidateResolved(resolved));
        }

        [Test]
        public void Should_Reject_A_Conditional_Chain_6_Deep()
        {
            var resolved = BuildChain(6);

            var ex = Assert.Throws<EventException>(() => _validator.ValidateResolved(resolved));

            Assert.That(ex.Message, Is.EqualTo(PremiumErrorCodes.EventQuestionDepthExceeded));
        }

        /// <summary>
        /// Question 0 is always shown (depth 0); each subsequent question is triggered by the
        /// previous question's option, so question N sits at depth N.
        /// </summary>
        private static List<ResolvedEventQuestionDto> BuildChain(int depth)
        {
            var questions = new List<ResolvedEventQuestionDto>
            {
                new ResolvedEventQuestionDto { QuestionId = 1, Order = 0, ShowIfOptionId = null, OptionIds = new[] { 10 } }
            };

            for (var i = 1; i <= depth; i++)
            {
                questions.Add(new ResolvedEventQuestionDto
                {
                    QuestionId = i + 1,
                    Order = i,
                    ShowIfOptionId = (i * 10),
                    OptionIds = new[] { (i + 1) * 10 }
                });
            }

            return questions;
        }
    }
}
