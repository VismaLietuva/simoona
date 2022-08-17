using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;

namespace Shrooms.Contracts.DataTransferObjects.Wall.Comments
{
    public class CommentDto
    {
        public int Id { get; set; }

        public string MessageBody { get; set; }

        public DateTime Created { get; set; }

        public bool IsLiked { get; set; }

        public IEnumerable<LikeDto> Likes { get; set; }

        public IEnumerable<string> Images { get; set; }

        public int PostId { get; set; }

        public UserDto Author { get; set; }

        public bool CanModerate { get; set; }

        public bool IsEdited { get; set; }

        public bool IsHidden { get; set; }

        public DateTime LastEdit { get; set; }
    }
}
