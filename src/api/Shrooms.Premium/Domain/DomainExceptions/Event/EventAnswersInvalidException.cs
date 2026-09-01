using System.Collections.Generic;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.DomainExceptions.Event
{
    /// <summary>
    /// Thrown when sign-up answers do not satisfy the question tree. Carries a machine-readable
    /// error list so the attendee wizard can jump to the offending step instead of showing a
    /// generic message. Derives from <see cref="EventException"/> so existing catch blocks still
    /// work if a caller has not been updated.
    /// </summary>
    public class EventAnswersInvalidException : EventException
    {
        public EventAnswersInvalidException(IReadOnlyList<EventAnswerErrorDto> errors)
            : base(PremiumErrorCodes.EventAnswersInvalid)
        {
            Errors = errors;
        }

        public IReadOnlyList<EventAnswerErrorDto> Errors { get; }
    }
}
