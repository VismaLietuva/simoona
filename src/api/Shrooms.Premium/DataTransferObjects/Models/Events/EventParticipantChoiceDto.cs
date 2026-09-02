namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventParticipantChoiceDto
    {
        /// <summary>Null for a legacy flat option, matching <c>EventOption.QuestionId</c>.</summary>
        public int? QuestionId { get; set; }

        public string Option { get; set; }
        public int Order { get; set; }
    }
}
