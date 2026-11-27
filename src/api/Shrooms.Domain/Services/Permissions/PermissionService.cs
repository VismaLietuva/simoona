using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MoreLinq;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Permissions
{
    public class PermissionService : IPermissionService
    {
        private const int LookupBatchSize = 500;

        private readonly DbSet<Permission> _permissionsDbSet;
        private readonly DbSet<IdentityUserRole<string>> _userRolesDbSet;
        private readonly DbSet<ApplicationUser> _usersDbSet;
        private readonly ICustomCache<string, IEnumerable<string>> _permissionsCache;

        public PermissionService(IUnitOfWork2 unitOfWork, ICustomCache<string, IEnumerable<string>> permissionsCache)
        {
            _permissionsDbSet = unitOfWork.GetDbSet<Permission>();
            _userRolesDbSet = unitOfWork.GetDbSet<IdentityUserRole<string>>();
            _usersDbSet = unitOfWork.GetDbSet<ApplicationUser>();
            _permissionsCache = permissionsCache;
        }

        public bool UserHasPermission(UserAndOrganizationDto userAndOrg, string permissionName)
        {
            if (!_permissionsCache.TryGetValue(userAndOrg.UserId, out var permissions))
            {
                // In EF Core: Permission -> Role -> UserRole -> User
                permissions = (from p in _permissionsDbSet
                              from r in p.Roles
                              join ur in _userRolesDbSet on r.Id equals ur.RoleId
                              where ur.UserId == userAndOrg.UserId
                              select p)
                    .Where(FilterActiveModules(userAndOrg.OrganizationId))
                    .Select(x => x.Name)
                    .Distinct()
                    .ToList();

                _permissionsCache.TryAdd(userAndOrg.UserId, permissions);
            }

            return permissions.Contains(permissionName);
        }

        public async Task<bool> UserHasPermissionAsync(UserAndOrganizationDto userAndOrg, string permissionName, CancellationToken cancellationToken = default)
        {
            if (!_permissionsCache.TryGetValue(userAndOrg.UserId, out var permissions))
            {
                // In EF Core: Permission -> Role -> UserRole -> User
                permissions = await (from p in _permissionsDbSet
                                    from r in p.Roles
                                    join ur in _userRolesDbSet on r.Id equals ur.RoleId
                                    where ur.UserId == userAndOrg.UserId
                                    select p)
                    .Where(FilterActiveModules(userAndOrg.OrganizationId))
                    .Select(x => x.Name)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                _permissionsCache.TryAdd(userAndOrg.UserId, permissions);
            }

            var isPermitted = permissions.Contains(permissionName);
            return isPermitted;
        }

        public async Task<IEnumerable<PermissionGroupDto>> GetGroupNamesAsync(int organizationId)
        {
            var allPermissions = await GetPermissionsAsync(organizationId);

            return allPermissions
                .Select(x => new PermissionGroupDto
                {
                    Name = PermissionGroups.GetGroupName(x.Name)
                })
                .DistinctBy(x => x.Name)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId, int organizationId, CancellationToken cancellationToken = default)
        {
            if (_permissionsCache.TryGetValue(userId, out var permissions))
            {
                return permissions;
            }

            // Use join with IdentityUserRole junction table since ApplicationRole.Users navigation property doesn't exist in EF Core
            var permissionsQuery = (from p in _permissionsDbSet
                                   from r in p.Roles
                                   join ur in _userRolesDbSet on r.Id equals ur.RoleId
                                   where ur.UserId == userId
                                   select p)
                .Where(FilterActiveModules(organizationId));

            permissions = (await permissionsQuery.Distinct().ToListAsync(cancellationToken)).Select(x => x.Name).ToList();

            return permissions;
        }

        public Expression<Func<ApplicationUser, bool>> OnlyUsersWithPermission(string permissionName)
        {
            return user => _userRolesDbSet.Any(userRole =>
                userRole.UserId == user.Id &&
                _permissionsDbSet.Any(permission =>
                    permission.Name == permissionName &&
                    permission.Roles.Any(role => role.Id == userRole.RoleId) &&
                    (!permission.ModuleId.HasValue || permission.Module.Organizations.Any(organization => organization.Id == user.OrganizationId))));
        }

        public Task<IList<string>> FilterEmailsByPermissionAsync(IEnumerable<string> emails, string permissionName)
        {
            return KeepPermittedAsync(emails, batch => _usersDbSet
                .Where(OnlyUsersWithPermission(permissionName))
                .Where(user => batch.Contains(user.Email))
                .Select(user => user.Email)
                .ToListAsync());
        }

        public Task<IList<string>> FilterUserIdsByPermissionAsync(IEnumerable<string> userIds, string permissionName)
        {
            return KeepPermittedAsync(userIds, batch => _usersDbSet
                .Where(OnlyUsersWithPermission(permissionName))
                .Where(user => batch.Contains(user.Id))
                .Select(user => user.Id)
                .ToListAsync());
        }

        private static async Task<IList<string>> KeepPermittedAsync(IEnumerable<string> values, Func<string[], Task<List<string>>> loadPermitted)
        {
            var requested = values.ToList();
            var permitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var candidates = requested
                .Where(value => !string.IsNullOrEmpty(value))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var batch in candidates.Chunk(LookupBatchSize))
            {
                permitted.UnionWith(await loadPermitted(batch));
            }

            return requested
                .Where(value => !string.IsNullOrEmpty(value) && permitted.Contains(value))
                .ToList();
        }

        public async Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(string roleId, int organizationId)
        {
            Expression<Func<Permission, bool>> roleFilter = x => x.Roles.Any(y => y.Id == roleId);

            return await GetPermissionsAsync(organizationId, roleFilter);
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsAsync(int organizationId, CancellationToken cancellationToken = default)
        {
            return await GetPermissionsAsync(organizationId, null, cancellationToken);
        }

        private async Task<IEnumerable<PermissionDto>> GetPermissionsAsync(int organizationId, Expression<Func<Permission, bool>> roleFilter, CancellationToken cancellationToken = default)
        {
            return await _permissionsDbSet
                .Include(x => x.Module.Organizations)
                .Include(x => x.Roles)
                .Where(roleFilter ?? (x => true))
                .Where(FilterActiveModules(organizationId))
                .Select(x => new PermissionDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Scope = x.Scope
                })
                .ToListAsync(cancellationToken);
        }

        private static Expression<Func<Permission, bool>> FilterActiveModules(int organizationId)
        {
            return x => !x.ModuleId.HasValue || x.Module.Organizations.Any(y => y.Id == organizationId);
        }

        public void RemoveCache(string userId)
        {
            _permissionsCache.TryRemoveEntry(userId);
        }
    }
}