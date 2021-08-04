using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;

namespace Shrooms.Domain.Services.Wall.Posts.Comments
{
    public interface ICommentService
    {
        Task EditCommentAsync(EditCommentDto commentDto);

        Task<CommentCreatedDto> CreateCommentAsync(NewCommentDto commentDto);

        Task ToggleLikeAsync(int commentId, UserAndOrganizationDto userOrg);

        Task DeleteCommentAsync(int id, UserAndOrganizationDto userOrg);

        Task<string> GetCommentBodyAsync(int id);

        Task HideCommentAsync(int id, UserAndOrganizationDto userOrg);

        Task DeleteCommentsByPostAsync(int postId);
    }
}