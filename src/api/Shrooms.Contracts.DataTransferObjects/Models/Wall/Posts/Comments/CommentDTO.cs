using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Models.Users;

namespace Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts.Comments
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string MessageBody { get; set; }
        public DateTime Created { get; set; }
        public bool IsLiked { get; set; }
        public IEnumerable<UserDto> Likes { get; set; }
        public string PictureId { get; set; }
        public int PostId { get; set; }
        public UserDto Author { get; set; }
        public bool CanModerate { get; set; }
        public bool IsEdited { get; set; }
        public bool IsHidden { get; set; }
        public DateTime LastEdit { get; set; }
    }
}
