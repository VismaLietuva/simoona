using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Models.Permissions;

namespace Shrooms.Domain.Services.Permissions
{
    /// <summary>
    /// The rules the role editor uses to show a role's permissions as groups and to turn the groups back into permissions.
    /// Reading a role (<c>Role/Get</c>) and saving it (<c>Role/Post</c>, <c>Role/Put</c>) both go through here,
    /// so a role that is read and saved unchanged keeps exactly the permissions it had.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A permission belongs to the group named by the part of its name before the first '_', lower-cased:
    /// <c>KUDOS_BASIC</c> is in <c>kudos</c>, and <c>KUDOSSHOP_ADMINISTRATION</c> is in <c>kudosshop</c>, not in <c>kudos</c>.
    /// </para>
    /// <para>
    /// The editor gives each group one scope: none, <see cref="PermissionScopes.Basic"/> or <see cref="PermissionScopes.Administration"/>.
    /// Basic grants the group's basic-scoped permissions. Admin grants every permission in the group, so admin implies basic:
    /// a group the administrator switches to admin gets <c>X_BASIC</c> as well as <c>X_ADMINISTRATION</c>.
    /// A group whose scope the administrator leaves alone keeps the permissions it has, even when they are not exactly
    /// what that scope would grant (for example <c>X_ADMINISTRATION</c> without <c>X_BASIC</c>).
    /// </para>
    /// </remarks>
    public static class PermissionGroups
    {
        public static string GetGroupName(string permissionName)
        {
            return permissionName.Split(DataLayerConstants.PermissionSplitter)[0].ToLowerInvariant();
        }

        public static bool IsInGroup(string permissionName, string groupName)
        {
            return string.Equals(GetGroupName(permissionName), groupName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The scope the editor shows for a group: the narrowest scope whose grant includes every permission the role holds in it.
        /// Any permission that is not basic-scoped needs admin, because only admin grants it.
        /// </summary>
        /// <param name="heldPermissionsInGroup">The permissions the role holds in one group.</param>
        public static string GetActiveScope(IEnumerable<PermissionDto> heldPermissionsInGroup)
        {
            var held = heldPermissionsInGroup.ToList();

            if (held.Any(permission => !IsBasic(permission)))
            {
                return PermissionScopes.Administration;
            }

            return held.Count > 0 ? PermissionScopes.Basic : string.Empty;
        }

        /// <summary>
        /// Whether choosing <paramref name="activeScope"/> for a group grants <paramref name="permission"/>, a permission of that group.
        /// </summary>
        public static bool Grants(string activeScope, PermissionDto permission)
        {
            return activeScope switch
            {
                PermissionScopes.Administration => true,
                PermissionScopes.Basic => IsBasic(permission),
                _ => false
            };
        }

        /// <summary>
        /// The permissions a role holds after the editor saves <paramref name="requestedGroups"/>.
        /// </summary>
        /// <param name="editablePermissions">The permissions the editor shows for the organization (those of its active modules).</param>
        /// <param name="currentPermissionIds">The permissions the role holds now; empty for a new role.</param>
        /// <param name="requestedGroups">Every group with the scope the administrator chose. A group left out is set to none.</param>
        /// <returns>
        /// The ids of the permissions the role should hold. Permissions the editor does not show are kept. In each group,
        /// the role keeps what it holds when the requested scope equals the one <see cref="GetActiveScope"/> reports,
        /// and otherwise gets what the requested scope grants.
        /// </returns>
        public static IReadOnlyCollection<int> ResolvePermissionIds(
            IEnumerable<PermissionDto> editablePermissions,
            IEnumerable<int> currentPermissionIds,
            IEnumerable<PermissionGroupDto> requestedGroups)
        {
            var editable = editablePermissions.ToList();
            var editableIds = editable.Select(permission => permission.Id).ToHashSet();
            var currentIds = currentPermissionIds.ToHashSet();
            var requestedScopes = GetRequestedScopes(requestedGroups);

            var resolvedIds = currentIds.Where(id => !editableIds.Contains(id)).ToHashSet();

            foreach (var group in editable.GroupBy(permission => GetGroupName(permission.Name)))
            {
                var held = group.Where(permission => currentIds.Contains(permission.Id)).ToList();
                var requestedScope = requestedScopes.GetValueOrDefault(group.Key, string.Empty);

                var granted = requestedScope == GetActiveScope(held)
                    ? held
                    : group.Where(permission => Grants(requestedScope, permission));

                resolvedIds.UnionWith(granted.Select(permission => permission.Id));
            }

            return resolvedIds;
        }

        private static bool IsBasic(PermissionDto permission)
        {
            return permission.Scope == PermissionScopes.Basic;
        }

        private static Dictionary<string, string> GetRequestedScopes(IEnumerable<PermissionGroupDto> requestedGroups)
        {
            // A group sent twice gets the wider of its scopes, as it did when each entry was granted on its own.
            return (requestedGroups ?? Enumerable.Empty<PermissionGroupDto>())
                .Where(group => !string.IsNullOrEmpty(group?.Name))
                .GroupBy(group => group.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.Any(g => g.ActiveScope == PermissionScopes.Administration) ? PermissionScopes.Administration
                           : group.Any(g => g.ActiveScope == PermissionScopes.Basic) ? PermissionScopes.Basic
                           : string.Empty,
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}
