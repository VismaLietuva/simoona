using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.DataTransferObjects.Wall.Comments;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Wall.Posts
{
    public class PostDto
    {
        public int Id { get; set; }
        public string MessageBody { get; set; }
        public DateTime Created { get; set; }
        public bool IsLiked { get; set; }
        public string PictureId { get; set; }
        public UserDto Author { get; set; }
        public string SharedEventId { get; set; }
        public IEnumerable<CommentDto> Comments { get; set; }
        public IEnumerable<LikeDto> Likes { get; set; }
        public int WallId { get; set; }
        public string WallName { get; set; }
        public WallType WallType { get; set; }
        public DateTime LastActivity { get; set; }
        public bool CanModerate { get; set; }
        public bool IsEdited { get; set; }
        public bool IsHidden { get; set; }
        public DateTime Modified { get; set; }
        public DateTime LastEdit { get; set; }
        public bool IsWatched { get; set; }
    }
}
