using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;
using Shrooms.Premium.Domain.DomainServiceValidators.Events;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    public class EventAnswerValidatorTests
    {
        private EventAnswerValidator _validator;

        [SetUp]
        public void TestInitializer()
        {
            _validator = new EventAnswerValidator();
        }

        /// <summary>
        /// q1 "Pick your dish" (required, single): options 10 Pizza, 11 Pasta.
        /// q2 "Which pizza?"   (required, single, shown if 10): options 20, 21.
        /// q3 "Anything else?" (optional, multi, always shown): options 30, 31.
        /// </summary>
        private static List<ResolvedEventQuestionDto> FoodTree()
        {
            return new List<ResolvedEventQuestionDto>
            {
                new ResolvedEventQuestionDto
                {
                    QuestionId = 1, Order = 0, SelectType = EventQuestionSelectType.Single,
                    IsRequired = true, ShowIfOptionId = null, OptionIds = new[] { 10, 11 }
                },
                new ResolvedEventQuestionDto
                {
                    QuestionId = 2, Order = 1, SelectType = EventQuestionSelectType.Single,
                    IsRequired = true, ShowIfOptionId = 10, OptionIds = new[] { 20, 21 }
                },
                new ResolvedEventQuestionDto
                {
                    QuestionId = 3, Order = 2, SelectType = EventQuestionSelectType.Multi,
                    IsRequired = false, ShowIfOptionId = null, OptionIds = new[] { 30, 31 }
                }
            };
        }

        [Test]
        public void Should_Accept_A_Complete_Branch()
        {
            Assert.DoesNotThrow(() => _validator.Validate(FoodTree(), new[] { 10, 20 }, new int[0]));
        }

        [Test]
        public void Should_Accept_A_Branch_That_Skips_The_Conditional_Question()
        {
            // Pasta chosen, so "Which pizza?" is not reachable and needs no answer.
            Assert.DoesNotThrow(() => _validator.Validate(FoodTree(), new[] { 11 }, new int[0]));
        }

        [Test]
        public void Should_Reject_An_Option_That_Does_Not_Belong_To_The_Event()
        {
            var ex = Assert.Throws<EventAnswersInvalidException>(
                () => _validator.Validate(FoodTree(), new[] { 10, 20, 999 }, new int[0]));

            Assert.That(ex.Errors.Single().Reason, Is.EqualTo(EventAnswerErrorReason.UnknownOption));
            Assert.That(ex.Errors.Single().QuestionId, Is.Null);
        }

        [Test]
        public void Should_Reject_Two_Answers_To_A_Single_Select_Question()
        {
            var ex = Assert.Throws<EventAnswersInvalidException>(
                () => _validator.Validate(FoodTree(), new[] { 10, 11, 20 }, new int[0]));

            Assert.That(ex.Errors.Any(e => e.QuestionId == 1 && e.Reason == EventAnswerErrorReason.TooManyAnswers), Is.True);
        }

        [Test]
        public void Should_Accept_Two_Answers_To_A_Multi_Select_Question()
        {
            Assert.DoesNotThrow(() => _validator.Validate(FoodTree(), new[] { 11, 30, 31 }, new int[0]));
        }

        [Test]
        public void Should_Reject_A_Missing_Answer_To_A_Reachable_Required_Question()
        {
            // Pizza chosen but "Which pizza?" left unanswered.
            var ex = Assert.Throws<EventAnswersInvalidException>(
                () => _validator.Validate(FoodTree(), new[] { 10 }, new int[0]));

            Assert.That(ex.Errors.Single().QuestionId, Is.EqualTo(2));
            Assert.That(ex.Errors.Single().Reason, Is.EqualTo(EventAnswerErrorReason.RequiredAnswerMissing));
        }

        [Test]
        public void Should_Reject_An_Answer_To_A_Hidden_Question()
        {
            // Pasta chosen, yet a pizza sub-option was answered.
            var ex = Assert.Throws<EventAnswersInvalidException>(
                () => _validator.Validate(FoodTree(), new[] { 11, 20 }, new int[0]));

            Assert.That(ex.Errors.Single().QuestionId, Is.EqualTo(2));
            Assert.That(ex.Errors.Single().Reason, Is.EqualTo(EventAnswerErrorReason.AnswerForHiddenQuestion));
        }

        [Test]
        public void Should_Report_Every_Failing_Question_Not_Just_The_First()
        {
            var questions = new List<ResolvedEventQuestionDto>
            {
                new ResolvedEventQuestionDto
                {
                    QuestionId = 1, Order = 0, SelectType = EventQuestionSelectType.Single,
                    IsRequired = true, ShowIfOptionId = null, OptionIds = new[] { 10 }
                },
                new ResolvedEventQuestionDto
                {
                    QuestionId = 2, Order = 1, SelectType = EventQuestionSelectType.Single,
                    IsRequired = true, ShowIfOptionId = null, OptionIds = new[] { 20 }
                }
            };

            var ex = Assert.Throws<EventAnswersInvalidException>(
                () => _validator.Validate(questions, new int[0], new int[0]));

            Assert.That(ex.Errors.Count, Is.EqualTo(2));
        }

        [Test]
        public void Should_Treat_A_Question_As_Hidden_When_Its_Trigger_Question_Is_Itself_Hidden()
        {
            // q3 is triggered by an option of q2, but q2 is hidden because q1 chose 11.
            var questions = FoodTree();
            questions.Add(new ResolvedEventQuestionDto
            {
                QuestionId = 4, Order = 3, SelectType = EventQuestionSelectType.Single,
                IsRequired = true, ShowIfOptionId = 20, OptionIds = new[] { 40 }
            });

            var ex = Assert.Throws<EventAnswersInvalidException>(
                () => _validator.Validate(questions, new[] { 11, 40 }, new int[0]));

            Assert.That(ex.Errors.Any(e => e.QuestionId == 4 && e.Reason == EventAnswerErrorReason.AnswerForHiddenQuestion), Is.True);
        }

        [Test]
        public void Should_Accept_Legacy_Flat_Options_Alongside_Questions()
        {
            Assert.DoesNotThrow(() => _validator.Validate(FoodTree(), new[] { 11, 500 }, new[] { 500, 501 }));
        }

        [Test]
        public void Should_Accept_Legacy_Only_Events_With_No_Questions()
        {
            Assert.DoesNotThrow(() => _validator.Validate(new List<ResolvedEventQuestionDto>(), new[] { 500 }, new[] { 500 }));
        }
    }
}
