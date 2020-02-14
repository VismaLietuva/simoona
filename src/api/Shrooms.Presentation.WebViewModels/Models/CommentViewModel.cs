using System;
using System.Collections.Generic;
using Shrooms.Presentation.WebViewModels.Models.Like;

namespace Shrooms.Presentation.WebViewModels.Models
{
    public class CommentViewModel : AbstractViewModel
    {
        public string MessageBody { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public bool IsLiked { get; set; }

        public ICollection<LikeViewModel> Likes { get; set; }

        public int PostId { get; set; }

        public UserViewModel User { get; set; }

        public string PictureId { get; set; }
    }
}