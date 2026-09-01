using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventAnswerErrorDto
    {
        /// <summary>The question the wizard should open on.</summary>
        public int QuestionId { get; set; }

        public EventAnswerErrorReason Reason { get; set; }
    }
}
