using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Administration;
using Shrooms.EntityModels.Models;

namespace Shrooms.Domain.Services.Administration
{
    public interface IAdministrationUsersService
    {
        bool UserEmailExists(string email);

        byte[] GetAllUsersExcel();

        void ConfirmNewUser(string userId, UserAndOrganizationDTO userAndOrg);

        bool HasExistingExternalLogin(string email, string loginProvider);

        Task<IdentityResult> CreateNewUserWithExternalLogin(ExternalLoginInfo info, string requestedOrganization);

        Task<IdentityResult> CreateNewUser(ApplicationUser user, string password, string requestedOrganization);

        IEnumerable<AdministrationUserDTO> GetAllUsers(string sortQuery, string search, FilterDTO[] filter, string includeProperties);

        bool UserIsSoftDeleted(string email);

        void RestoreUser(string email);

        Task AddProviderImage(string userId, ClaimsIdentity externalIdentity);

        void NotifyAboutNewUser(ApplicationUser user, int orgId);

        void SetUserTutorialStatusToComplete(string userId);

        bool GetUserTutorialStatus(string userId);

        void AddProviderEmail(string userId, string provider, string email);

        Task SendUserPasswordResetEmail(ApplicationUser user, string organizationName);

        Task SendUserVerificationEmail(ApplicationUser user, string organizationName);
    }
}
