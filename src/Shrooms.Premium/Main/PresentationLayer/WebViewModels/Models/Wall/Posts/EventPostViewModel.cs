using System;
using System.Collections.Generic;
using Shrooms.WebViewModels.Models;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Wall.Posts
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
