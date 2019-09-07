using System;
using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.EntityModels.Models;

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
        IEnumerable<string> GetPostWatchersIds(int postId, string excludedUserId);
        IEnumerable<ApplicationUser> GetPostWatchers(int postId, string excludedUserId);
    }
}
