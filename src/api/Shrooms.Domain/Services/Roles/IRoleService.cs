using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Roles;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Roles
{
    public interface IRoleService
    {
        Expression<Func<ApplicationUser, bool>> ExcludeUsersWithRole(string roleName);

        IEnumerable<string> GetRoleIdsByNames(params string[] names);

        IEnumerable<RoleDTO> GetRolesForAutocomplete(string search, UserAndOrganizationDTO userAndOrg);

        RoleDetailsDTO GetRoleById(UserAndOrganizationDTO userAndOrganizationDTO, string roleId);

        IList<string> GetAdministrationRoleEmails(int orgId);

        bool HasRole(string userId, string roleName);
    }
}