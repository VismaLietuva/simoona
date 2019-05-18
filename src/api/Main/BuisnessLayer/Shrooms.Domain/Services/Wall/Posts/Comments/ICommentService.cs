using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Wall.Posts.Comments;

namespace Shrooms.Domain.Services.Wall.Posts.Comments
{
    public interface ICommentService
    {
        void EditComment(EditCommentDTO commentDto);
        CommentCreatedDTO CreateComment(NewCommentDTO commentDto);
        void ToggleLike(int commentId, UserAndOrganizationDTO userOrg);
        IList<string> GetCommentsAuthorsToNotify(int postId, IEnumerable<string> excludeUsers);
        void DeleteComment(int id, UserAndOrganizationDTO userOrg);
        void HideComment(int id, UserAndOrganizationDTO userOrg);
        void DeleteCommentsByPost(int postId, UserAndOrganizationDTO userAndOrg);
        bool IsPostAuthorAppNotificationsEnabled(string userId);
    }
}
