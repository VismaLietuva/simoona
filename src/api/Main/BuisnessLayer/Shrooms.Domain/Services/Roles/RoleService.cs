using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Permissions;
using Shrooms.DataTransferObjects.Models.Roles;
using Shrooms.Domain.Services.Permissions;
using Shrooms.EntityModels.Models;

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

        public Expression<Func<ApplicationUser, bool>> ExcludeUsersWithRole(string roleName)
        {
            var roleId = GetRoleIdByName(roleName);

            return x => !x.Roles.Any(y => y.RoleId == roleId);
        }

        public IEnumerable<string> GetRoleIdsByNames(params string[] names)
        {
            return _roleDbSet.Where(x => names.Any(n => n == x.Name)).Select(x => x.Id);
        }

        public IEnumerable<RoleDTO> GetRolesForAutocomplete(string search, UserAndOrganizationDTO userOrg)
        {
            return _roleDbSet
                .Where(x => x.OrganizationId == userOrg.OrganizationId &&
                            x.Name.Contains(search))
                .Select(x => new RoleDTO { Id = x.Id, Name = x.Name })
                .ToList();
        }

        public IList<string> GetAdministrationRoleEmails(int orgId)
        {
            var administrationRole = GetRole(role => role.Name == Constants.Authorization.Roles.Administration && role.OrganizationId == orgId, orgId, true);

            if (administrationRole == null || !administrationRole.Users.Any())
            {
                return new List<string>();
            }

            return administrationRole.Users.Select(s => s.Email).ToList();
        }

        public RoleDetailsDTO GetRoleById(UserAndOrganizationDTO userAndOrganizationDTO, string roleId)
        {
            return GetRole(role => role.Id == roleId, userAndOrganizationDTO.OrganizationId);
        }

        public bool HasRole(string userId, string roleName)
        {
            return _roleDbSet
                .Include(x => x.Users)
                .Any(x => x.Name == roleName && 
                          x.Users.Any(u => u.UserId == userId));
        }

        private RoleDetailsDTO GetRole(Expression<Func<ApplicationRole, bool>> roleFilter, int orgId, bool skipPermission = false)
        {
            var role = _roleDbSet
                .Where(roleFilter)
                .Select(x => new RoleDetailsDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .Single();

            role.Users = GetUsersWithRole(role.Id);

            if (!skipPermission)
            {
                role.Permissions = GetGroupNamesByRole(orgId, role.Id);
            }

            return role;
        }

        private string GetRoleIdByName(string roleName)
        {
            return _roleDbSet
                            .Where(x => x.Name == roleName)
                            .Select(x => x.Id)
                            .FirstOrDefault();
        }

        private IEnumerable<RoleUserDTO> GetUsersWithRole(string roleId)
        {
            return _userDbSet
                .Where(x => x.Roles.Any(y => y.RoleId == roleId))
                .Select(x => new RoleUserDTO
                {
                    Id = x.Id,
                    Email = x.Email,
                    FullName = x.FirstName + " " + x.LastName
                })
                .ToList();
        }

        private IEnumerable<PermissionGroupDTO> GetGroupNamesByRole(int orgId, string roleId)
        {
            var groupNames = _permissionService.GetGroupNames(orgId);
            var rolePermissions = _permissionService.GetRolePermissions(roleId, orgId);
            var groupNamesWithScopes = groupNames
                .Select(x => new PermissionGroupDTO
                {
                    Name = x.Name,
                    ActiveScope = rolePermissions.Any(y => y.Name.StartsWith(x.Name, StringComparison.OrdinalIgnoreCase) && y.Scope == Scopes.Administration)
                                  ? Scopes.Administration
                                  : (rolePermissions.Any(y => y.Name.StartsWith(x.Name, StringComparison.OrdinalIgnoreCase) && y.Scope == Scopes.Basic)
                                        ? Scopes.Basic
                                        : string.Empty)
                })
                .ToList();

            return groupNamesWithScopes;
        }
    }
}
