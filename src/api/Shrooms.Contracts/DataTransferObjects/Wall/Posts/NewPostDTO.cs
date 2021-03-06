﻿using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Wall.Posts
{
    public class NewPostDTO : UserAndOrganizationDTO
    {
        public string MessageBody { get; set; }
        public string PictureId { get; set; }
        public string SharedEventId { get; set; }
        public int WallId { get; set; }

        public IEnumerable<string> MentionedUserIds { get; set; }
    }
}
