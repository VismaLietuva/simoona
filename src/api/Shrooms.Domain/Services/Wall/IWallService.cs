using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Wall;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using MultiwallWall = Shrooms.DataLayer.EntityModels.Models.Multiwall.Wall;

namespace Shrooms.Domain.Services.Wall
{
    public interface IWallService
    {
        Task<ApplicationUserMinimalDto> JoinOrLeaveWallAsync(int wallId, string attendeeId, string actorId, int tenantId, bool isEventWall);

        Task UpdateWallAsync(UpdateWallDto updateWallDto);

        Task<IEnumerable<WallMemberDto>> GetWallMembersAsync(int wallId, UserAndOrganizationDto userOrg);

        Task<WallDto> GetWallAsync(int wallId, UserAndOrganizationDto userOrg);

        Task<WallDto> GetWallDetailsAsync(int wallId, UserAndOrganizationDto userOrg);

        Task<IEnumerable<WallDto>> GetWallsListAsync(UserAndOrganizationDto userOrg, WallsListFilter filter);

        Task<PostDto> GetWallPostAsync(UserAndOrganizationDto userAndOrg, int postId);

        Task DeleteWallAsync(int wallId, UserAndOrganizationDto userOrg, WallType type);

        Task<int> CreateNewWallAsync(CreateWallDto newWall);

        Task<IEnumerable<string>> GetWallMembersIdsAsync(int wallId, UserAndOrganizationDto userOrg);

        Task<IEnumerable<PostDto>> GetWallPostsAsync(int pageNumber, int pageSize, UserAndOrganizationDto userOrg, int? wallId);

        Task<IEnumerable<PostDto>> SearchWallAsync(string searchString, UserAndOrganizationDto userAndOrg, int pageNumber, int pageSize);

        Task RemoveModeratorAsync(int wallId, string responsibleUserId, UserAndOrganizationDto userAndOrg);

        Task AddModeratorAsync(int wallId, string responsibleUserId, UserAndOrganizationDto userId);

        Task<IEnumerable<PostDto>> GetAllPostsAsync(int page, int defaultPageSize, UserAndOrganizationDto userAndOrg, WallsListFilter filter);

        Task AddMemberToWallsAsync(string userId, List<int> wallIds);

        Task RemoveMemberFromWallAsync(string userId, int wallId);

        Task RemoveMemberFromWallsAsync(string userId, List<int> wallIds);

        Task ReplaceMembersInWallAsync(IEnumerable<ApplicationUser> newMembers, int wallId, string currentUserId);
        
        Task CheckIfUserIsAllowedToModifyWallContentAsync(
            MultiwallWall wall,
            string createdBy,
            string permission,
            UserAndOrganizationDto userOrg,
            bool checkForAdministrationEventPermission = true);
    }
}