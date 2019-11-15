using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.DataTransferObjects.Models.Wall.Posts;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;

namespace Shrooms.Domain.Services.Wall
{
    public interface IWallService
    {
        ApplicationUserMinimalViewModelDto JoinLeaveWall(int wallId, string attendeeId, string actorId, int tenantId, bool isEventWall);

        void UpdateWall(UpdateWallDto updateWallDto);

        Task<IEnumerable<WallMemberDto>> GetWallMembers(int wallId, UserAndOrganizationDTO userOrg);

        Task<WallDto> WallDetails(int wallId, UserAndOrganizationDTO userOrg);

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