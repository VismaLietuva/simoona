namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventOptionCountDto
    {
        public string Option { get; set; }

        public int Count { get; set; }

        /// <summary>
        /// Null for a legacy flat option, which belongs to no sign-up question.
        /// </summary>
        public int? QuestionId { get; set; }

        public string Question { get; set; }
    }
}
