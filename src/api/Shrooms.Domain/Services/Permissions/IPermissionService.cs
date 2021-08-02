using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;

namespace Shrooms.Domain.Services.Permissions
{
    public interface IPermissionService
    {
        Task<bool> UserHasPermissionAsync(UserAndOrganizationDTO userAndOrg, string permissionName);
        Task<IEnumerable<PermissionGroupDTO>> GetGroupNamesAsync(int organizationId);
        Task<IEnumerable<string>> GetUserPermissionsAsync(string userId, int organizationId);
        Task<IEnumerable<PermissionDTO>> GetRolePermissionsAsync(string roleId, int organizationId);
        void RemoveCache(string userId);
        bool UserHasPermission(UserAndOrganizationDTO userAndOrg, string permissionName);
    }
}
