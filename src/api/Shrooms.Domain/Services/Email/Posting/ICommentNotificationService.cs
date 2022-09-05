using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;

namespace Shrooms.Domain.Services.Email.Posting
{
    public interface ICommentNotificationService
    {
        Task NotifyAboutNewCommentAsync(CommentCreatedDto commentDto);

        Task NotifyMentionedUsersAsync(EditCommentDto editCommentDto);
    }
}
