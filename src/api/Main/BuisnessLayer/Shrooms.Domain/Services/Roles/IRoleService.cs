using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Roles;
using Shrooms.EntityModels.Models;

namespace Shrooms.Domain.Services.Roles
{
    public interface IRoleService
    {
        Expression<Func<ApplicationUser, bool>> ExcludeUsersWithRole(string roleName);

        IEnumerable<RoleDTO> GetRolesForAutocomplete(string search, UserAndOrganizationDTO userAndOrg);

        RoleDetailsDTO GetRoleById(UserAndOrganizationDTO userAndOrganizationDTO, string roleId);

        IList<string> GetAdministrationRoleEmails(int orgId);

        bool HasRole(string userId, string roleName);
    }
}