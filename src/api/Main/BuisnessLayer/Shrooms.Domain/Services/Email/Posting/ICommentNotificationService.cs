using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;

namespace Shrooms.Domain.Services.Email.Posting
{
    public interface ICommentNotificationService
    {
        void NotifyAboutNewComment(Comment comment, ApplicationUser commentCreator);
    }
}
