using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventAnswerErrorDto
    {
        /// <summary>Null when the failure has no owning question, as for an unknown option.</summary>
        public int? QuestionId { get; set; }

        /// <summary>Set when the failure names a specific option, as for an unknown option.</summary>
        public int? OptionId { get; set; }

        public EventAnswerErrorReason Reason { get; set; }
    }
}
