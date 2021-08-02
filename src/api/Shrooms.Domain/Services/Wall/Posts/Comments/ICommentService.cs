using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;

namespace Shrooms.Domain.Services.Wall.Posts.Comments
{
    public interface ICommentService
    {
        Task EditCommentAsync(EditCommentDTO commentDto);

        Task<CommentCreatedDTO> CreateCommentAsync(NewCommentDTO commentDto);

        void ToggleLike(int commentId, UserAndOrganizationDTO userOrg);

        Task DeleteCommentAsync(int id, UserAndOrganizationDTO userOrg);

        Task<string> GetCommentBodyAsync(int id);

        Task HideCommentAsync(int id, UserAndOrganizationDTO userOrg);

        Task DeleteCommentsByPostAsync(int postId);
    }
}