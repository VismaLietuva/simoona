using Shrooms.Contracts.DataTransferObjects.Wall.Posts;

namespace Shrooms.Domain.Services.Email.Posting
{
    public interface IPostNotificationService
    {
        void NotifyAboutNewPost(NewlyCreatedPostDTO post);
    }
}
