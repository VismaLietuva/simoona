using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;

namespace Shrooms.Domain.Services.Email.Posting
{
    public interface IPostNotificationService
    {
        Task NotifyAboutNewPostAsync(NewlyCreatedPostDto post);

        Task NotifyMentionedUsersAsync(EditPostDto editPostDto);
    }
}
