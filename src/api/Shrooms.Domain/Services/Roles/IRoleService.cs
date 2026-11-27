using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;
using Shrooms.Contracts.DataTransferObjects.Models.Roles;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Roles
{
    public interface IRoleService
    {
        Expression<Func<ApplicationUser, bool>> ExcludeUsersWithRole(string roleId);

        Expression<Func<ApplicationUser, bool>> ExcludeUsersWithRoleName(string roleName);

        Expression<Func<ApplicationUser, bool>> OnlyUsersWithRoleName(string roleName);

        Task<IEnumerable<RoleDto>> GetRolesForAutocompleteAsync(string search, UserAndOrganizationDto userAndOrg);

        Task<RoleDetailsDto> GetRoleByIdAsync(UserAndOrganizationDto userAndOrganizationDto, string roleId);

        /// <summary>
        /// The permissions a role should hold after the role editor saves <paramref name="requestedGroups"/>,
        /// using the same group rules as <see cref="GetRoleByIdAsync"/>; see <see cref="Permissions.PermissionGroups"/>.
        /// </summary>
        /// <param name="currentPermissionIds">The permissions the role holds now; empty for a new role.</param>
        Task<IReadOnlyCollection<int>> GetPermissionIdsToAssignAsync(int orgId, IEnumerable<int> currentPermissionIds, IEnumerable<PermissionGroupDto> requestedGroups);

        Task<IList<string>> GetAdministrationRoleEmailsAsync(int orgId);

        Task<bool> HasRoleAsync(string userId, string roleName);

        Task<string> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken = default);
    }
}
