using System;
using System.Collections.Generic;
using Shrooms.Contracts.ViewModels.User;
using Shrooms.Contracts.ViewModels.Wall.Likes;

namespace Shrooms.Contracts.ViewModels.Wall.Posts
{
    public class WallPostViewModel
    {
        public int Id { get; set; }

        public string MessageBody { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public bool IsLiked { get; set; }

        public IEnumerable<LikeViewModel> Likes { get; set; }

        public UserViewModel Author { get; set; }

        public IEnumerable<CommentViewModel> Comments { get; set; }

        public string PictureId { get; set; }

        public string SharedEventId { get; set; }

        public int WallId { get; set; }

        public string WallName { get; set; }

        public DateTime LastActivity { get; set; }

        public bool CanModerate { get; set; }

        public bool IsEdited { get; set; }

        public bool IsHidden { get; set; }

        public DateTime Modified { get; set; }

        public DateTime LastEdit { get; set; }

        public bool IsWatched { get; set; }
    }
}