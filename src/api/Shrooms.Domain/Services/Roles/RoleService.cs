using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;
using Shrooms.Contracts.DataTransferObjects.Models.Roles;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;

namespace Shrooms.Domain.Services.Roles
{
    public class RoleService : IRoleService
    {
        private readonly IDbSet<ApplicationRole> _roleDbSet;
        private readonly IDbSet<ApplicationUser> _userDbSet;

        private readonly IPermissionService _permissionService;

        public RoleService(IUnitOfWork2 uow, IPermissionService permissionService)
        {
            _roleDbSet = uow.GetDbSet<ApplicationRole>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _permissionService = permissionService;
        }

        public Expression<Func<ApplicationUser, bool>> ExcludeUsersWithRole(string roleId)
        {
            return x => x.Roles.All(y => y.RoleId != roleId);
        }

        public async Task<IEnumerable<RoleDTO>> GetRolesForAutocompleteAsync(string search, UserAndOrganizationDTO userOrg)
        {
            return await _roleDbSet
                .Where(x => x.OrganizationId == userOrg.OrganizationId && x.Name.Contains(search))
                .Select(x => new RoleDTO { Id = x.Id, Name = x.Name })
                .ToListAsync();
        }

        public async Task<IList<string>> GetAdministrationRoleEmailsAsync(int orgId)
        {
            var administrationRole = await GetRoleAsync(role => role.Name == Contracts.Constants.Roles.Administration && role.OrganizationId == orgId, orgId, true);

            if (administrationRole == null || !administrationRole.Users.Any())
            {
                return new List<string>();
            }

            return administrationRole.Users.Select(s => s.Email).ToList();
        }

        public async Task<RoleDetailsDTO> GetRoleByIdAsync(UserAndOrganizationDTO userAndOrganizationDTO, string roleId)
        {
            return await GetRoleAsync(role => role.Id == roleId, userAndOrganizationDTO.OrganizationId);
        }

        public async Task<bool> HasRoleAsync(string userId, string roleName)
        {
            return await _roleDbSet
                .Include(x => x.Users)
                .AnyAsync(x => x.Name == roleName && x.Users.Any(u => u.UserId == userId));
        }

        public async Task<string> GetRoleIdByNameAsync(string roleName)
        {
            return await _roleDbSet
                .Where(x => x.Name == roleName)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<RoleDetailsDTO> GetRoleAsync(Expression<Func<ApplicationRole, bool>> roleFilter, int orgId, bool skipPermission = false)
        {
            var role = await _roleDbSet
                .Where(roleFilter)
                .Select(x => new RoleDetailsDTO
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .SingleAsync();

            role.Users = await GetUsersWithRoleAsync(role.Id);

            if (!skipPermission)
            {
                role.Permissions = await GetGroupNamesByRoleAsync(orgId, role.Id);
            }

            return role;
        }

        private async Task<IEnumerable<RoleUserDTO>> GetUsersWithRoleAsync(string roleId)
        {
            return await _userDbSet
                .Where(x => x.Roles.Any(y => y.RoleId == roleId))
                .Select(x => new RoleUserDTO
                {
                    Id = x.Id,
                    Email = x.Email,
                    FullName = x.FirstName + " " + x.LastName
                })
                .ToListAsync();
        }

        private async Task<IEnumerable<PermissionGroupDTO>> GetGroupNamesByRoleAsync(int orgId, string roleId)
        {
            var groupNames = await _permissionService.GetGroupNamesAsync(orgId);
            var rolePermissions = (await _permissionService.GetRolePermissionsAsync(roleId, orgId)).ToList();

            var groupNamesWithScopes = groupNames
                .Select(x => new PermissionGroupDTO
                {
                    Name = x.Name,
                    ActiveScope = rolePermissions.Any(y => y.Name.StartsWith(x.Name, StringComparison.OrdinalIgnoreCase) && y.Scope == PermissionScopes.Administration)
                                      ? PermissionScopes.Administration
                                      : (rolePermissions.Any(y => y.Name.StartsWith(x.Name, StringComparison.OrdinalIgnoreCase) && y.Scope == PermissionScopes.Basic)
                                             ? PermissionScopes.Basic
                                             : string.Empty)
                })
                .ToList();

            return groupNamesWithScopes;
        }
    }
}