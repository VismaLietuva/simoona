using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.Contracts.DataTransferObjects.Models.Users;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Organizations
{
    public interface IOrganizationService
    {
        Organization GetOrganizationById(int id);

        bool RequiresUserConfirmation(int organizationId);

        Organization GetOrganizationByName(string organizationName);

        Organization GetUserOrganization(ApplicationUser user);

        string GetOrganizationHostName(string organizationName);

        bool HasOrganizationEmailDomainRestriction(string organizationName);

        bool IsOrganizationHostValid(string email, string organizationName);

        UserDto GetManagingDirector(int organizationId);
        void SetManagingDirector(string userId, UserAndOrganizationDTO userAndOrganizationDTO);
    }
}
