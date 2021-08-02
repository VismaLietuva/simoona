using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Wall.Posts
{
    public interface IPostService
    {
        Task EditPostAsync(EditPostDTO editPostDto);
        Task<NewlyCreatedPostDTO> CreateNewPostAsync(NewPostDTO newPostDto);
        Task ToggleLikeAsync(int postId, UserAndOrganizationDTO userOrg);
        string GetPostBody(int postId);
        Task DeleteWallPostAsync(int postId, UserAndOrganizationDTO userOrg);
        Task HideWallPostAsync(int postId, UserAndOrganizationDTO userOrg);
        Task ToggleWatchAsync(int postId, UserAndOrganizationDTO userAndOrg, bool shouldWatch);
        Task<IList<string>> GetPostWatchersForAppNotificationsAsync(int postId);
        Task<IList<ApplicationUser>> GetPostWatchersForEmailNotificationsAsync(int postId);
    }
}
