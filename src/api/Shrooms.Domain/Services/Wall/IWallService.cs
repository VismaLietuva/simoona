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
        ApplicationUserMinimalDto JoinLeaveWall(int wallId, string attendeeId, string actorId, int tenantId, bool isEventWall);

        void UpdateWall(UpdateWallDto updateWallDto);

        Task<IEnumerable<WallMemberDto>> GetWallMembers(int wallId, UserAndOrganizationDTO userOrg);

        Task<WallDto> GetWall(int wallId, UserAndOrganizationDTO userOrg);

        Task<WallDto> GetWallDetails(int wallId, UserAndOrganizationDTO userOrg);

        Task<IEnumerable<WallDto>> GetWallsList(UserAndOrganizationDTO userOrg, WallsListFilter filter);

        Task<PostDTO> GetWallPost(UserAndOrganizationDTO userAndOrg, int postId);

        void DeleteWall(int wallId, UserAndOrganizationDTO userOrg, WallType type);

        Task<int> CreateNewWall(CreateWallDto newWall);

        IEnumerable<string> GetWallMembersIds(int wallId, UserAndOrganizationDTO userOrg);

        Task<IEnumerable<PostDTO>> GetWallPosts(int pageNumber, int pageSize, UserAndOrganizationDTO userOrg, int? wallId);

        Task<IEnumerable<PostDTO>> SearchWall(string searchString, UserAndOrganizationDTO userAndOrg, int pageNumber, int pageSize);

        void RemoveModerator(int wallId, string responsibleUserId, UserAndOrganizationDTO userAndOrg);

        void AddModerator(int wallId, string responsibleUserId, UserAndOrganizationDTO userId);

        Task<IEnumerable<PostDTO>> GetAllPosts(int page, int defaultPageSize, UserAndOrganizationDTO userAndOrg, int wallsType);

        void AddMemberToWalls(string userId, List<int> wallIds);

        void RemoveMemberFromWall(string userId, int wallId);

        void RemoveMemberFromWalls(string userId, List<int> wallIds);

        void ReplaceMembersInWall(IEnumerable<ApplicationUser> newMembers, int wallId, string currentUserId);
    }
}