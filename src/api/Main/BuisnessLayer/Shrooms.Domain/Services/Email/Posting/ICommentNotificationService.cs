using Shrooms.DataTransferObjects.Models.Wall.Posts.Comments;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;

namespace Shrooms.Domain.Services.Email.Posting
{
    public interface ICommentNotificationService
    {
        void NotifyAboutNewComment(Comment comment, ApplicationUser commentCreator);
        void NotifyAboutNewComment(CommentCreatedDTO commentDto);
    }
}
