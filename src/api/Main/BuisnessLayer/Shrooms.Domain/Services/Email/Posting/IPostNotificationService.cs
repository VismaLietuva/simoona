using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;

namespace Shrooms.Domain.Services.Email.Posting
{
    public interface IPostNotificationService
    {
        void NotifyAboutNewPost(Post post, ApplicationUser postCreator);
        void NotifyAboutNewPost(NewlyCreatedPostDTO post);
    }
}
