using System.Collections.Generic;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Events
{
    public interface IEventQuestionStructureValidator
    {
        void ValidatePayload(IList<EventQuestionStructureDto> questions);

        void ValidateResolved(IReadOnlyList<ResolvedEventQuestionDto> questions);
    }
}
