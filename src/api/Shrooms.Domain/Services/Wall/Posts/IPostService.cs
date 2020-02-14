using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Posts;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Wall.Posts
{
    public interface IPostService
    {
        void EditPost(EditPostDTO editPostDto);
        NewlyCreatedPostDTO CreateNewPost(NewPostDTO newPostDto);
        void ToggleLike(int postId, UserAndOrganizationDTO userOrg);
        void DeleteWallPost(int postId, UserAndOrganizationDTO userOrg);
        void HideWallPost(int postId, UserAndOrganizationDTO userOrg);
        void ToggleWatch(int postId, UserAndOrganizationDTO userAndOrg, bool shouldWatch);
        IEnumerable<string> GetPostWatchersForAppNotifications(int postId);
        IEnumerable<ApplicationUser> GetPostWatchersForEmailNotifications(int postId);
    }
}
