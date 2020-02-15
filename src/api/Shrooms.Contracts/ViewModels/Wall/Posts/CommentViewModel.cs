using System;
using System.Collections.Generic;
using Shrooms.Contracts.ViewModels.User;

namespace Shrooms.Contracts.ViewModels.Wall.Posts
{
    public class CommentViewModel
    {
        public int Id { get; set; }

        public string MessageBody { get; set; }

        public DateTime Created { get; set; }

        public bool IsLiked { get; set; }

        public string PictureId { get; set; }

        public int PostId { get; set; }

        public IEnumerable<UserViewModel> Likes { get; set; }

        public UserViewModel Author { get; set; }

        public bool CanModerate { get; set; }

        public bool IsEdited { get; set; }

        public DateTime LastEdit { get; set; }

        public bool IsHidden { get; set; }
    }
}