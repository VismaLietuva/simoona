namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventParticipantChoiceDto
    {
        public int OptionId { get; set; }

        /// <summary>Null for a legacy flat option.</summary>
        public int? QuestionOrder { get; set; }

        public string Option { get; set; }

        public int Order { get; set; }
    }
}
