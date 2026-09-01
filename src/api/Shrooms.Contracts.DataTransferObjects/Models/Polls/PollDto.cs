using System;
using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Models.Polls
{
    public class PollDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsAnonymous { get; set; }

        public bool IsOfficial { get; set; }

        public DateTime Deadline { get; set; }

        public DateTime? ClosedAt { get; set; }

        public DateTime Created { get; set; }

        public PollState State { get; set; }

        public int WallId { get; set; }

        public PollPersonDto CreatedBy { get; set; }

        public PollReviewDto Review { get; set; }

        public int VoterCount { get; set; }

        public int AudienceSize { get; set; }

        public bool VotedByMe { get; set; }

        public bool CanSeeResults { get; set; }

        public int QuestionCount { get; set; }

        public IEnumerable<PollQuestionDto> Questions { get; set; }
    }
}
