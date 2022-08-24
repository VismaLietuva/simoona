using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Wall.Posts
{
    public interface IPostService
    {
        Task EditPostAsync(EditPostDto editPostDto);

        Task<NewlyCreatedPostDto> CreateNewPostAsync(NewPostDto newPostDto);

        Task ToggleLikeAsync(AddLikeDto addLikeDto, UserAndOrganizationDto userOrg);

        Task<string> GetPostBodyAsync(int postId);

        Task DeleteWallPostAsync(int postId, UserAndOrganizationDto userOrg);

        Task HideWallPostAsync(int postId, UserAndOrganizationDto userOrg);

        Task ToggleWatchAsync(int postId, UserAndOrganizationDto userAndOrg, bool shouldWatch);

        Task<IList<string>> GetPostWatchersForAppNotificationsAsync(int postId);

        Task<IList<ApplicationUser>> GetPostWatchersForEmailNotificationsAsync(int postId);

        Task<ApplicationUserDto> GetPostCreatorByIdAsync(int postId);
    }
}
