using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using MoreLinq;
using Shrooms.Constants.DataLayer;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Permissions;
using Shrooms.EntityModels.Models;
using Shrooms.Infrastructure.CustomCache;
using Shrooms.Host.Contracts.DAL;

namespace Shrooms.Domain.Services.Permissions
{
    public class PermissionService : IPermissionService
    {
        private readonly IDbSet<Permission> _permissionsDbSet;
        private readonly ICustomCache<string, IEnumerable<string>> _permissionsCache;

        public PermissionService(IUnitOfWork2 unitOfWork, ICustomCache<string, IEnumerable<string>> permissionsCache)
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

        public IEnumerable<PermissionGroupDTO> GetGroupNames(int organizationId)
        {
            var allPermissions = GetPermissions(organizationId);

            return allPermissions
                .Select(x => new PermissionGroupDTO
                {
                    Name = x.Name.Split(ConstDataLayer.PermissionSplitter).First().ToLower()
                })
                .DistinctBy(x => x.Name)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public IEnumerable<string> GetUserPermissions(string userId, int organizationId)
        {
            IEnumerable<string> permissions;
            if (_permissionsCache.TryGetValue(userId, out permissions))
            {
                return permissions;
            }

            Expression<Func<Permission, bool>> userFilter =
                p => p.Roles.Any(r => r.Users.Any(u => u.UserId == userId));

            permissions = GetPermissions(organizationId, userFilter).Select(x => x.Name).ToList();

            return permissions;
        }

        public IEnumerable<PermissionDTO> GetRolePermissions(string roleId, int organizationId)
        {
            Expression<Func<Permission, bool>> roleFilter = x => x.Roles.Any(y => y.Id == roleId);

            return GetPermissions(organizationId, roleFilter);
        }

        private IEnumerable<PermissionDTO> GetPermissions(int organizationId, Expression<Func<Permission, bool>> roleFilter = null)
        {
            return _permissionsDbSet
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
                .ToList();
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