using System.Collections.Generic;
using System.Linq;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Events
{
    /// <summary>
    /// Write-time structural rules for the sign-up question tree. Pure — no database access —
    /// so it can be unit tested directly.
    /// </summary>
    public class EventQuestionStructureValidator : IEventQuestionStructureValidator
    {
        public const int MaxQuestionsPerEvent = 20;
        public const int MaxOptionsPerQuestion = 30;
        public const int MaxTitleLength = 100;
        public const int MaxOptionNameLength = 100;
        public const int MaxConditionalDepth = 5;

        /// <summary>
        /// Checks everything decidable before rows are inserted: limits, lengths, and that every
        /// condition names exactly one trigger that exists somewhere in this payload.
        /// </summary>
        public void ValidatePayload(IList<EventQuestionStructureDto> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                return;
            }

            if (questions.Count > MaxQuestionsPerEvent)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionLimitExceeded);
            }

            foreach (var question in questions)
            {
                // A JSON payload with "options": null deserializes to a null list, overriding the
                // DTO's field initializer. Normalise it here so nothing downstream — including the
                // SelectMany below — has to guard against a null Options list.
                question.Options ??= new List<EventQuestionOptionStructureDto>();

                ValidateQuestionShape(question);
            }

            var optionClientIds = questions
                .SelectMany(q => q.Options)
                .Where(o => o.ClientId != null)
                .Select(o => o.ClientId)
                .ToHashSet();

            foreach (var question in questions)
            {
                ValidateCondition(question, optionClientIds);
            }
        }

        /// <summary>
        /// Checks the rules that need real IDs: a condition must point at an option owned by a
        /// question with a strictly lower order, and the conditional chain must not exceed
        /// <see cref="MaxConditionalDepth"/>.
        /// </summary>
        public void ValidateResolved(IReadOnlyList<ResolvedEventQuestionDto> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                return;
            }

            var ordered = questions.OrderBy(q => q.Order).ToList();

            var ownerByOptionId = new Dictionary<int, ResolvedEventQuestionDto>();
            foreach (var question in ordered)
            {
                foreach (var optionId in question.OptionIds)
                {
                    ownerByOptionId[optionId] = question;
                }
            }

            var depthByQuestionId = new Dictionary<int, int>();

            foreach (var question in ordered)
            {
                if (question.ShowIfOptionId == null)
                {
                    depthByQuestionId[question.QuestionId] = 0;
                    continue;
                }

                if (!ownerByOptionId.TryGetValue(question.ShowIfOptionId.Value, out var owner) ||
                    owner.Order >= question.Order)
                {
                    throw new EventException(PremiumErrorCodes.EventQuestionConditionInvalid);
                }

                // The owner sits at a lower order, so it has already been assigned a depth.
                var depth = depthByQuestionId[owner.QuestionId] + 1;

                if (depth > MaxConditionalDepth)
                {
                    throw new EventException(PremiumErrorCodes.EventQuestionDepthExceeded);
                }

                depthByQuestionId[question.QuestionId] = depth;
            }
        }

        private static void ValidateQuestionShape(EventQuestionStructureDto question)
        {
            if (question.Id == null && string.IsNullOrWhiteSpace(question.ClientId))
            {
                throw new EventException(PremiumErrorCodes.EventQuestionClientIdMissing);
            }

            if (string.IsNullOrWhiteSpace(question.Title) || question.Title.Length > MaxTitleLength)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionTitleInvalid);
            }

            // Normalised to a non-null list in ValidatePayload before this is called.
            var options = question.Options;

            if (options.Count > MaxOptionsPerQuestion)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionOptionLimitExceeded);
            }

            foreach (var option in options)
            {
                if (string.IsNullOrWhiteSpace(option.Name) || option.Name.Length > MaxOptionNameLength)
                {
                    throw new EventException(PremiumErrorCodes.EventQuestionOptionNameInvalid);
                }
            }
        }

        private static void ValidateCondition(EventQuestionStructureDto question, HashSet<string> optionClientIds)
        {
            var hasId = question.ShowIfOptionId != null;
            var hasClientId = !string.IsNullOrWhiteSpace(question.ShowIfOptionClientId);

            if (hasId && hasClientId)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionConditionAmbiguous);
            }

            if (hasClientId && !optionClientIds.Contains(question.ShowIfOptionClientId))
            {
                throw new EventException(PremiumErrorCodes.EventQuestionConditionInvalid);
            }
        }
    }
}
