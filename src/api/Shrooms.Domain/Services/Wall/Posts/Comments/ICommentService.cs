using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall.Comments;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;

namespace Shrooms.Domain.Services.Wall.Posts.Comments
{
    public interface ICommentService
    {
        /// <summary>Returns the users newly mentioned by this edit, to notify.</summary>
        Task<IEnumerable<string>> EditCommentAsync(EditCommentDto commentDto);

        Task<CommentCreatedDto> CreateCommentAsync(NewCommentDto commentDto);

        Task ToggleLikeAsync(AddLikeDto addLikeDto, UserAndOrganizationDto userOrg);

        Task DeleteCommentAsync(int id, UserAndOrganizationDto userOrg);

        Task<string> GetCommentBodyAsync(int id);

        Task HideCommentAsync(int id, UserAndOrganizationDto userOrg);

        Task DeleteCommentsByPostAsync(int postId);

        Task<MentionCommentDto> GetMentionCommentByIdAsync(int commentId);
    }
}