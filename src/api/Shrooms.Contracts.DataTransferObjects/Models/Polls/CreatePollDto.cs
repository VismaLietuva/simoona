using System;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Polls
{
    public class CreatePollDto : UserAndOrganizationDto
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsAnonymous { get; set; }

        public bool IsOfficial { get; set; }

        public DateTime Deadline { get; set; }

        public bool Publish { get; set; }

        public bool Suggest { get; set; }

        public IList<CreatePollQuestionDto> Questions { get; set; }
    }
}
