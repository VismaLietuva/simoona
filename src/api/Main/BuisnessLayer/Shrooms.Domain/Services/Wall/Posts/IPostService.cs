using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Wall.Posts;

namespace Shrooms.Domain.Services.Wall.Posts
{
    public interface IPostService
    {
        void EditPost(EditPostDTO editPostDto);
        NewlyCreatedPostDTO CreateNewPost(NewPostDTO newPostDto);
        void ToggleLike(int postId, UserAndOrganizationDTO userOrg);
        void DeleteWallPost(int postId, UserAndOrganizationDTO userOrg);
        void HideWallPost(int postId, UserAndOrganizationDTO userOrg);
    }
}
