using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;

namespace Shrooms.Domain.Services.Email.Posting
{
    public interface ICommentEmailNotificationService
    {
        Task SendEmailNotificationAsync(CommentCreatedDto commentDto);
    }
}
