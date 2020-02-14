using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts.Comments;

namespace Shrooms.Domain.Services.Email.Posting
{
    public interface ICommentEmailNotificationService
    {
        void SendEmailNotification(CommentCreatedDTO commentDto);
    }
}
