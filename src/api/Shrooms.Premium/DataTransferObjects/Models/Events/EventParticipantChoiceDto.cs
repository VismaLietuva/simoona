namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventParticipantChoiceDto
    {
        public int OptionId { get; set; }

        /// <summary>Null for a legacy flat option, matching <c>EventOption.QuestionId</c>.</summary>
        public int? QuestionId { get; set; }

        public int? QuestionOrder { get; set; }

        public string Option { get; set; }

        public int Order { get; set; }
    }
}
