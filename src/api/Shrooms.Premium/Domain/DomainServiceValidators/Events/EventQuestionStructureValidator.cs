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
        public void ValidatePayload(IList<EventQuestionStructureDto> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                return;
            }

            if (questions.Any(question => question == null))
            {
                throw new EventException(PremiumErrorCodes.EventQuestionPayloadInvalid);
            }

            if (questions.Count > EventsConstants.EventQuestionsMaxCount)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionLimitExceeded);
            }

            foreach (var question in questions)
            {
                // A JSON payload with "options": null deserializes to a null list, overriding the
                // DTO's field initializer. Normalise it here so nothing downstream — including the
                // SelectMany below — has to guard against a null Options list.
                question.Options ??= new List<EventQuestionOptionStructureDto>();

                if (question.Options.Any(option => option == null))
                {
                    throw new EventException(PremiumErrorCodes.EventQuestionPayloadInvalid);
                }

                ValidateQuestionShape(question);
            }

            ValidateUniqueness(questions);

            // ValidateUniqueness has rejected duplicate clientIds by now, so one owner per
            // clientId holds and the order is enough to place the trigger in the tree.
            var ownerOrderByOptionClientId = questions
                .SelectMany(question => question.Options
                    .Select(option => new { option.ClientId, OwnerOrder = question.Order }))
                .Where(entry => !string.IsNullOrWhiteSpace(entry.ClientId))
                .ToDictionary(entry => entry.ClientId, entry => entry.OwnerOrder);

            foreach (var question in questions)
            {
                ValidateCondition(question, ownerOrderByOptionClientId);
            }
        }

        /// <summary>
        /// Checks the rules that need real IDs: a condition must point at an option owned by a
        /// question with a strictly lower order, and the conditional chain must not exceed
        /// <see cref="EventsConstants.EventQuestionMaxConditionalDepth"/>.
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

                if (depth > EventsConstants.EventQuestionMaxConditionalDepth)
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

            if (string.IsNullOrWhiteSpace(question.Title) ||
                question.Title.Length > EventsConstants.EventQuestionTitleMaxLength)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionTitleInvalid);
            }

            var options = question.Options;

            // A question with no options can never be answered, so a required one would reject
            // every join for good.
            if (options.Count < EventsConstants.EventQuestionOptionsMinCount)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionOptionsMissing);
            }

            if (options.Count > EventsConstants.EventQuestionOptionsMaxCount)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionOptionLimitExceeded);
            }

            foreach (var option in options)
            {
                if (string.IsNullOrWhiteSpace(option.Name) ||
                    option.Name.Length > EventsConstants.EventQuestionOptionNameMaxLength)
                {
                    throw new EventException(PremiumErrorCodes.EventQuestionOptionNameInvalid);
                }
            }

            if (HasDuplicates(options.Select(option => option.Order)))
            {
                throw new EventException(PremiumErrorCodes.EventQuestionDuplicateOrder);
            }
        }

        /// <summary>
        /// Identities have to be unique across the whole payload, not just within one question.
        /// A repeated question id collapses two payload entries onto one row, and a repeated
        /// clientId makes a condition bind to an arbitrary one of the options claiming it.
        /// </summary>
        private static void ValidateUniqueness(IList<EventQuestionStructureDto> questions)
        {
            var options = questions.SelectMany(question => question.Options).ToList();

            if (HasDuplicates(questions.Where(q => q.Id != null).Select(q => q.Id.Value)) ||
                HasDuplicates(options.Where(o => o.Id != null).Select(o => o.Id.Value)))
            {
                throw new EventException(PremiumErrorCodes.EventQuestionDuplicateId);
            }

            var clientIds = questions
                .Select(question => question.ClientId)
                .Concat(options.Select(option => option.ClientId))
                .Where(clientId => !string.IsNullOrWhiteSpace(clientId));

            if (HasDuplicates(clientIds))
            {
                throw new EventException(PremiumErrorCodes.EventQuestionDuplicateClientId);
            }

            // Every read projection sorts on Order with no secondary key, so ties would reorder
            // the wizard's steps between requests.
            if (HasDuplicates(questions.Select(question => question.Order)))
            {
                throw new EventException(PremiumErrorCodes.EventQuestionDuplicateOrder);
            }
        }

        private static void ValidateCondition(
            EventQuestionStructureDto question,
            IReadOnlyDictionary<string, int> ownerOrderByOptionClientId)
        {
            var hasId = question.ShowIfOptionId != null;
            var hasClientId = !string.IsNullOrWhiteSpace(question.ShowIfOptionClientId);

            if (hasId && hasClientId)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionConditionAmbiguous);
            }

            if (!hasClientId)
            {
                return;
            }

            // Same rule as ValidateResolved, applied before the insert: a trigger has to belong to
            // a question at a strictly lower order. Without the order check a self- or
            // forward-reference only fails in ValidateResolved, which by definition runs once the
            // rows already carry ids.
            if (!ownerOrderByOptionClientId.TryGetValue(question.ShowIfOptionClientId, out var ownerOrder) ||
                ownerOrder >= question.Order)
            {
                throw new EventException(PremiumErrorCodes.EventQuestionConditionInvalid);
            }
        }

        private static bool HasDuplicates<T>(IEnumerable<T> values)
        {
            var seen = new HashSet<T>();
            return values.Any(value => !seen.Add(value));
        }
    }
}
