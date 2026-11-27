using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Permissions
{
    public interface IPermissionService
    {
        Task<bool> UserHasPermissionAsync(UserAndOrganizationDto userAndOrg, string permissionName, CancellationToken cancellationToken = default);
        Task<IEnumerable<PermissionGroupDto>> GetGroupNamesAsync(int organizationId);
        Task<IEnumerable<string>> GetUserPermissionsAsync(string userId, int organizationId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(string roleId, int organizationId);
        Task<IEnumerable<PermissionDto>> GetPermissionsAsync(int organizationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Users holding <paramref name="permissionName"/> through any of their roles, the same way
        /// <see cref="GetUserPermissionsAsync"/> decides it. Use it inside a users query.
        /// </summary>
        Expression<Func<ApplicationUser, bool>> OnlyUsersWithPermission(string permissionName);

        /// <summary>
        /// The given emails whose users hold <paramref name="permissionName"/>, in the order given.
        /// Emails of users without it, or of no user at all, are left out.
        /// </summary>
        Task<IList<string>> FilterEmailsByPermissionAsync(IEnumerable<string> emails, string permissionName);

        /// <summary>
        /// The given user ids whose users hold <paramref name="permissionName"/>, in the order given.
        /// </summary>
        Task<IList<string>> FilterUserIdsByPermissionAsync(IEnumerable<string> userIds, string permissionName);

        void RemoveCache(string userId);
        bool UserHasPermission(UserAndOrganizationDto userAndOrg, string permissionName);
    }
}
