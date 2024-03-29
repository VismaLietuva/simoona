﻿using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts
{
    public class EditPostDto : UserAndOrganizationDto
    {
        public int Id { get; set; }

        public string MessageBody { get; set; }

        public IEnumerable<string> Images { get; set; }

        public IEnumerable<string> MentionedUserIds { get; set; }
    }
}
