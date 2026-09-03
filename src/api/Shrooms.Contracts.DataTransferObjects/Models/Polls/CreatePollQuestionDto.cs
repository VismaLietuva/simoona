using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Polls
{
    public class CreatePollQuestionDto
    {
        public string Text { get; set; }

        public bool AllowMultiple { get; set; }

        public IList<CreatePollOptionDto> Options { get; set; }
    }
}
