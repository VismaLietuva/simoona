using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Polls
{
    public class PollOptionDto
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public int VoteCount { get; set; }

        public bool Picked { get; set; }

        public IEnumerable<PollPersonDto> Voters { get; set; }
    }
}
