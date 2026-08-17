using System.Collections.Generic;
using System.Linq;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.DomainExceptions.Event;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Events
{
    /// <summary>
    /// Answer-time rules for the sign-up question tree. Pure — no database access.
    /// Collects every failure rather than throwing on the first, so the wizard can show them all.
    /// </summary>
    public class EventAnswerValidator : IEventAnswerValidator
    {
        public void Validate(
            IReadOnlyList<ResolvedEventQuestionDto> questions,
            IReadOnlyCollection<int> chosenOptionIds,
            IReadOnlyCollection<int> legacyOptionIds)
        {
            var chosen = (chosenOptionIds ?? new int[0]).ToHashSet();
            var legacy = (legacyOptionIds ?? new int[0]).ToHashSet();
            var ordered = (questions ?? new List<ResolvedEventQuestionDto>()).OrderBy(q => q.Order).ToList();

            var errors = new List<EventAnswerErrorDto>();

            var knownOptionIds = ordered.SelectMany(q => q.OptionIds).Concat(legacy).ToHashSet();

            foreach (var unknown in chosen.Where(id => !knownOptionIds.Contains(id)))
            {
                errors.Add(new EventAnswerErrorDto
                {
                    QuestionId = null,
                    Reason = EventAnswerErrorReason.UnknownOption
                });
            }

            // Triggers always live at a lower order, so a single forward pass resolves
            // reachability: by the time a question is visited, its trigger's owner is settled.
            var reachableByQuestionId = new Dictionary<int, bool>();
            var ownerByOptionId = new Dictionary<int, int>();

            foreach (var question in ordered)
            {
                foreach (var optionId in question.OptionIds)
                {
                    ownerByOptionId[optionId] = question.QuestionId;
                }
            }

            foreach (var question in ordered)
            {
                var reachable = IsReachable(question, chosen, ownerByOptionId, reachableByQuestionId);
                reachableByQuestionId[question.QuestionId] = reachable;

                var answeredHere = question.OptionIds.Count(chosen.Contains);

                if (!reachable)
                {
                    if (answeredHere > 0)
                    {
                        errors.Add(new EventAnswerErrorDto
                        {
                            QuestionId = question.QuestionId,
                            Reason = EventAnswerErrorReason.AnswerForHiddenQuestion
                        });
                    }

                    continue;
                }

                if (question.SelectType == EventQuestionSelectType.Single && answeredHere > 1)
                {
                    errors.Add(new EventAnswerErrorDto
                    {
                        QuestionId = question.QuestionId,
                        Reason = EventAnswerErrorReason.TooManyAnswers
                    });
                }

                if (question.IsRequired && answeredHere == 0)
                {
                    errors.Add(new EventAnswerErrorDto
                    {
                        QuestionId = question.QuestionId,
                        Reason = EventAnswerErrorReason.RequiredAnswerMissing
                    });
                }
            }

            if (errors.Count > 0)
            {
                throw new EventAnswersInvalidException(errors);
            }
        }

        private static bool IsReachable(
            ResolvedEventQuestionDto question,
            HashSet<int> chosen,
            IReadOnlyDictionary<int, int> ownerByOptionId,
            IReadOnlyDictionary<int, bool> reachableByQuestionId)
        {
            if (question.ShowIfOptionId == null)
            {
                return true;
            }

            var triggerId = question.ShowIfOptionId.Value;

            if (!chosen.Contains(triggerId))
            {
                return false;
            }

            // The trigger was chosen, but it only counts if the question owning it was itself
            // shown — otherwise a hidden branch would resurrect its children.
            return ownerByOptionId.TryGetValue(triggerId, out var ownerId) &&
                   reachableByQuestionId.TryGetValue(ownerId, out var ownerReachable) &&
                   ownerReachable;
        }
    }
}
