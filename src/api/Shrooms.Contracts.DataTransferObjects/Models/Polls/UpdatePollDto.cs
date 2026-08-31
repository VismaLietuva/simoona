using System;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Polls
{
    public class UpdatePollDto : UserAndOrganizationDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsAnonymous { get; set; }

        public bool IsOfficial { get; set; }

        public DateTime Deadline { get; set; }

        public IList<CreatePollQuestionDto> Questions { get; set; }
    }
}
