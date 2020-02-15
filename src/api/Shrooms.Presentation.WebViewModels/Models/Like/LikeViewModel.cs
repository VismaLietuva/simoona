using Shrooms.Contracts.ViewModels;
using Shrooms.Contracts.ViewModels.Wall.Posts;

namespace Shrooms.Presentation.WebViewModels.Models.Like
{
    public class LikeViewModel : AbstractViewModel
    {
        public string LikeUserId { get; set; }

        public ApplicationUserViewModel LikeUser { get; set; }

        public int? LikedCommentId { get; set; }

        public CommentViewModel LikedComment { get; set; }

        public int? LikedPostId { get; set; }

        public WallPostViewModel LikedPost { get; set; }
    }
}