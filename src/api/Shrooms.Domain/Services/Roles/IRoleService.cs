using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Roles;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Roles
{
    public interface IRoleService
    {
        Expression<Func<ApplicationUser, bool>> ExcludeUsersWithRole(string roleId);

        Task<IEnumerable<RoleDTO>> GetRolesForAutocompleteAsync(string search, UserAndOrganizationDTO userAndOrg);

        Task<RoleDetailsDTO> GetRoleByIdAsync(UserAndOrganizationDTO userAndOrganizationDTO, string roleId);

        Task<IList<string>> GetAdministrationRoleEmailsAsync(int orgId);

        Task<bool> HasRoleAsync(string userId, string roleName);

        Task<string> GetRoleIdByNameAsync(string roleName);
    }
}