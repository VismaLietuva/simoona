using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Wall
{
    public interface IWallService
    {
        Task<ApplicationUserMinimalDto> JoinOrLeaveWallAsync(int wallId, string attendeeId, string actorId, int tenantId, bool isEventWall);

        Task UpdateWallAsync(UpdateWallDto updateWallDto);

        Task<IEnumerable<WallMemberDto>> GetWallMembersAsync(int wallId, UserAndOrganizationDTO userOrg);

        Task<WallDto> GetWallAsync(int wallId, UserAndOrganizationDTO userOrg);

        Task<WallDto> GetWallDetailsAsync(int wallId, UserAndOrganizationDTO userOrg);

        Task<IEnumerable<WallDto>> GetWallsListAsync(UserAndOrganizationDTO userOrg, WallsListFilter filter);

        Task<PostDTO> GetWallPostAsync(UserAndOrganizationDTO userAndOrg, int postId);

        Task DeleteWallAsync(int wallId, UserAndOrganizationDTO userOrg, WallType type);

        Task<int> CreateNewWallAsync(CreateWallDto newWall);

        Task<IEnumerable<string>> GetWallMembersIdsAsync(int wallId, UserAndOrganizationDTO userOrg);

        Task<IEnumerable<PostDTO>> GetWallPostsAsync(int pageNumber, int pageSize, UserAndOrganizationDTO userOrg, int? wallId);

        Task<IEnumerable<PostDTO>> SearchWallAsync(string searchString, UserAndOrganizationDTO userAndOrg, int pageNumber, int pageSize);

        Task RemoveModeratorAsync(int wallId, string responsibleUserId, UserAndOrganizationDTO userAndOrg);

        Task AddModeratorAsync(int wallId, string responsibleUserId, UserAndOrganizationDTO userId);

        Task<IEnumerable<PostDTO>> GetAllPostsAsync(int page, int defaultPageSize, UserAndOrganizationDTO userAndOrg, int wallsType);

        Task AddMemberToWallsAsync(string userId, List<int> wallIds);

        Task RemoveMemberFromWallAsync(string userId, int wallId);

        Task RemoveMemberFromWallsAsync(string userId, List<int> wallIds);

        Task ReplaceMembersInWallAsync(IEnumerable<ApplicationUser> newMembers, int wallId, string currentUserId);
    }
}