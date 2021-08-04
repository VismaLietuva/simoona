using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Organizations
{
    public interface IOrganizationService
    {
        Task<Organization> GetOrganizationByIdAsync(int id);

        Task<bool> RequiresUserConfirmationAsync(int organizationId);

        Task<Organization> GetOrganizationByNameAsync(string organizationName);

        Task<Organization> GetUserOrganizationAsync(ApplicationUser user);

        Task<string> GetOrganizationHostNameAsync(string organizationName);

        Task<bool> HasOrganizationEmailDomainRestrictionAsync(string organizationName);

        Task<bool> IsOrganizationHostValidAsync(string email, string organizationName);

        Task<UserDto> GetManagingDirectorAsync(int organizationId);
        Task SetManagingDirectorAsync(string userId, UserAndOrganizationDto userAndOrganizationDto);
    }
}
