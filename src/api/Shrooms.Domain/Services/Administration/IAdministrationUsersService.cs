using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Administration;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Administration
{
    public interface IAdministrationUsersService
    {
        Task<bool> UserEmailExistsAsync(string email);

        Task<byte[]> GetAllUsersExcelAsync();

        Task ConfirmNewUserAsync(string userId, UserAndOrganizationDTO userAndOrg);

        Task<bool> HasExistingExternalLoginAsync(string email, string loginProvider);

        Task<IdentityResult> CreateNewUserWithExternalLoginAsync(ExternalLoginInfo info, string requestedOrganization);

        Task<IdentityResult> CreateNewUserAsync(ApplicationUser user, string password, string requestedOrganization);

        Task<IEnumerable<AdministrationUserDTO>> GetAllUsersAsync(string sortQuery, string search, FilterDTO[] filter, string includeProperties);

        Task<bool> UserIsSoftDeletedAsync(string email);

        Task RestoreUserAsync(string email);

        Task AddProviderImageAsync(string userId, ClaimsIdentity externalIdentity);

        Task NotifyAboutNewUserAsync(ApplicationUser user, int orgId);

        Task SetUserTutorialStatusToCompleteAsync(string userId);

        Task<bool> GetUserTutorialStatusAsync(string userId);

        Task AddProviderEmailAsync(string userId, string provider, string email);

        Task SendUserPasswordResetEmailAsync(ApplicationUser user, string organizationName);

        Task SendUserVerificationEmailAsync(ApplicationUser user, string organizationName);
    }
}
