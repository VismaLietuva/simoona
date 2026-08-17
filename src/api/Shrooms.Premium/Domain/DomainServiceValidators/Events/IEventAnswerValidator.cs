using System.Collections.Generic;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Events
{
    public interface IEventAnswerValidator
    {
        void Validate(
            IReadOnlyList<ResolvedEventQuestionDto> questions,
            IReadOnlyCollection<int> chosenOptionIds,
            IReadOnlyCollection<int> legacyOptionIds);
    }
}
