using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;

namespace Shrooms.Domain.Services.Wall.Posts.Comments
{
    public interface ICommentService
    {
        void EditComment(EditCommentDTO commentDto);

        CommentCreatedDTO CreateComment(NewCommentDTO commentDto);

        void ToggleLike(int commentId, UserAndOrganizationDTO userOrg);

        void DeleteComment(int id, UserAndOrganizationDTO userOrg);

        string GetCommentBody(int id);

        void HideComment(int id, UserAndOrganizationDTO userOrg);

        void DeleteCommentsByPost(int postId);
    }
}