using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Wall.Posts
{
    public class NewlyCreatedPostDto
    {
        public int Id { get; set; }

        public string MessageBody { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public IEnumerable<string> Images { get; set; }

        public UserDto User { get; set; }

        public WallType WallType { get; set; }

        public int WallId { get; set; }

        public IEnumerable<string> MentionedUsersIds { get; set; }
    }
}
