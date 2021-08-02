using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MoreLinq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Permissions
{
    public class PermissionService : IPermissionService
    {
        private readonly IDbSet<Permission> _permissionsDbSet;
        private readonly ICustomCache<string, IList<string>> _permissionsCache;

        public PermissionService(IUnitOfWork2 unitOfWork, ICustomCache<string, IList<string>> permissionsCache)
        {
            _permissionsDbSet = unitOfWork.GetDbSet<Permission>();
            _permissionsCache = permissionsCache;
        }

        public bool UserHasPermission(UserAndOrganizationDTO userAndOrg, string permissionName)
        {
            if (!_permissionsCache.TryGetValue(userAndOrg.UserId, out var permissions))
            {
                permissions = _permissionsDbSet
                    .Where(p => p.Roles.Any(r => r.Users.Any(u => u.UserId == userAndOrg.UserId)))
                    .Where(FilterActiveModules(userAndOrg.OrganizationId))
                    .Select(x => x.Name)
                    .ToList();

                _permissionsCache.TryAdd(userAndOrg.UserId, permissions);
            }

            var isPermitted = permissions.Contains(permissionName);
            return isPermitted;
        }

        public async Task<bool> UserHasPermissionAsync(UserAndOrganizationDTO userAndOrg, string permissionName)
        {
            if (!_permissionsCache.TryGetValue(userAndOrg.UserId, out var permissions))
            {
                permissions = await _permissionsDbSet
                    .Where(p => p.Roles.Any(r => r.Users.Any(u => u.UserId == userAndOrg.UserId)))
                    .Where(FilterActiveModules(userAndOrg.OrganizationId))
                    .Select(x => x.Name)
                    .ToListAsync();

                _permissionsCache.TryAdd(userAndOrg.UserId, permissions);
            }

            var isPermitted = permissions.Contains(permissionName);
            return isPermitted;
        }

        public async Task<IEnumerable<PermissionGroupDTO>> GetGroupNamesAsync(int organizationId)
        {
            var allPermissions = await GetPermissionsAsync(organizationId);

            return allPermissions
                .Select(x => new PermissionGroupDTO
                {
                    Name = x.Name.Split(DataLayerConstants.PermissionSplitter).First().ToLower()
                })
                .DistinctBy(x => x.Name)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId, int organizationId)
        {
            if (_permissionsCache.TryGetValue(userId, out var permissions))
            {
                return permissions;
            }

            Expression<Func<Permission, bool>> userFilter = p => p.Roles.Any(r => r.Users.Any(u => u.UserId == userId));

            permissions = (await GetPermissionsAsync(organizationId, userFilter)).Select(x => x.Name).ToList();

            return permissions;
        }

        public async Task<IEnumerable<PermissionDTO>> GetRolePermissionsAsync(string roleId, int organizationId)
        {
            Expression<Func<Permission, bool>> roleFilter = x => x.Roles.Any(y => y.Id == roleId);

            return await GetPermissionsAsync(organizationId, roleFilter);
        }

        private async Task<IEnumerable<PermissionDTO>> GetPermissionsAsync(int organizationId, Expression<Func<Permission, bool>> roleFilter = null)
        {
            return await _permissionsDbSet
                .Include(x => x.Module.Organizations)
                .Include(x => x.Roles)
                .Where(roleFilter ?? (x => true))
                .Where(FilterActiveModules(organizationId))
                .Select(x => new PermissionDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Scope = x.Scope
                })
                .ToListAsync();
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