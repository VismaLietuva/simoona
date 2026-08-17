namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public enum EventAnswerErrorReason
    {
        UnknownOption,
        TooManyAnswers,
        RequiredAnswerMissing,
        AnswerForHiddenQuestion
    }

    public class EventAnswerErrorDto
    {
        /// <summary>
        /// Null only for <see cref="EventAnswerErrorReason.UnknownOption"/>, which by definition
        /// has no owning question.
        /// </summary>
        public int? QuestionId { get; set; }

        public EventAnswerErrorReason Reason { get; set; }
    }
}
