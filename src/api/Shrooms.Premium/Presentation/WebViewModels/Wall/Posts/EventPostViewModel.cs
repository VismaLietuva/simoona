using System;
using System.Collections.Generic;
using Shrooms.Contracts.ViewModels.User;
using Shrooms.Contracts.ViewModels.Wall.Posts;

namespace Shrooms.Premium.Presentation.WebViewModels.Wall.Posts
{
    public class EventPostViewModel
    {
        public int Id { get; set; }

        public string MessageBody { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public bool IsLiked { get; set; }

        public IEnumerable<UserViewModel> Likes { get; set; }

        public UserViewModel User { get; set; }

        public ICollection<CommentViewModel> Comments { get; set; }

        public string PictureId { get; set; }
    }
}
