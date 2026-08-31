using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Polls
{
    public class PollQuestionDto
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public bool AllowMultiple { get; set; }

        public int RespondentCount { get; set; }

        public IEnumerable<PollOptionDto> Options { get; set; }
    }
}
