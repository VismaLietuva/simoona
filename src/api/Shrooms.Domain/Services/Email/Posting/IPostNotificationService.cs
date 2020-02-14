using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.Domain.Services.Email.Posting
{
    public interface IPostNotificationService
    {
        void NotifyAboutNewPost(Post post, ApplicationUser postCreator);
        void NotifyAboutNewPost(NewlyCreatedPostDTO post);
    }
}
