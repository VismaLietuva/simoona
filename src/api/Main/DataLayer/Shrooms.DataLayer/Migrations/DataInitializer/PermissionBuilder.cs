using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Constants.Authorization;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.DataLayer.DAL;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.Constants;

namespace Shrooms.DataLayer.Migrations.DataInitializer
{
    public class PermissionBuilder
    {
        private readonly ShroomsDbContext _context;
        private readonly List<ApplicationRole> _roles;
        private readonly List<Module> _modules;

        private List<Permission> _currentPermissions;
        private List<Permission> _newPermissions;

        private DateTime _timestamp;

        public PermissionBuilder(ShroomsDbContext context)
        {
            _context = context;
            _roles = context.Roles.ToList();
            _modules = context.Modules.ToList();
            _currentPermissions = _context.Permissions.ToList();
            _newPermissions = new List<Permission>();
            _timestamp = DateTime.UtcNow;
        }

        public PermissionBuilder AddAdminPermission(string permissionName, string module = null, params string[] withRoleNames)
        {
            var roles = withRoleNames != null ?
                _roles.Where(x =>
                            withRoleNames.Any(y => y == x.Name) ||
                            x.Name == Roles.Admin)
                        .ToList() :
                _roles.Where(x =>
                            x.Name == Roles.Admin)
                        .ToList();

            var permission = new Permission()
            {
                Scope = Scopes.Administration,
                Name = permissionName,
                Created = _timestamp,
                Modified = _timestamp,
                Roles = roles,
                ModuleId = !string.IsNullOrEmpty(module) ? (int?)_modules.Single(m => m.Name == module).Id : null
            };

            _newPermissions.Add(permission);
            return this;
        }

        public PermissionBuilder AddBasicPermission(string permissionName, string module = null, params string[] withRoleNames)
        {
            var roles = withRoleNames != null ?
                _roles.Where(x =>
                                withRoleNames.Any(y => y == x.Name) ||
                                x.Name == Roles.User ||
                                x.Name == Roles.Admin)
                            .ToList() :
                _roles.Where(x =>
                            x.Name == Roles.User ||
                            x.Name == Roles.Admin)
                        .ToList();

            var permission = new Permission()
            {
                Scope = Scopes.Basic,
                Name = permissionName,
                Created = _timestamp,
                Modified = _timestamp,
                Roles = roles,
                ModuleId = !string.IsNullOrEmpty(module) ? (int?)_modules.Single(m => m.Name == module).Id : null
            };

            _newPermissions.Add(permission);
            return this;
        }

        public void UpdatePermissions()
        {
            RemoveUnnusedPermissions();
            AddUpdatePermissions();
            _context.SaveChanges(false);
        }

        private void RemoveUnnusedPermissions()
        {
            foreach (var currentPermission in _currentPermissions)
            {
                if (!_newPermissions.Any(x => x.Name == currentPermission.Name))
                {
                    _context.Permissions.Remove(currentPermission);
                }
            }
        }

        private void AddUpdatePermissions()
        {
            foreach (var newPermission in _newPermissions)
            {
                var match = _currentPermissions.SingleOrDefault(x => x.Name == newPermission.Name);
                if (match != null)
                {
                    match.ModuleId = newPermission.ModuleId;
                }
                else
                {
                    _context.Permissions.Add(newPermission);
                }
            }
        }
    }
}
