using System;

namespace Shrooms.Contracts.DataTransferObjects.Models.Polls
{
    public class PollReviewDto
    {
        public string Reason { get; set; }

        public DateTime At { get; set; }

        public PollPersonDto By { get; set; }
    }
}
