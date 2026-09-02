namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    /// <summary>
    /// One option a participant picked, either an answer to a sign-up question or, when
    /// <see cref="QuestionId"/> is null, a legacy flat option.
    /// </summary>
    public class EventParticipantChoiceDto
    {
        public int? QuestionId { get; set; }

        public string Option { get; set; }

        public int Order { get; set; }
    }
}
