using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Email.AdministrationUsers
{
    public interface IAdministrationNotificationService
    {
        void SendConfirmedNotificationEmail(string userEmail, UserAndOrganizationDTO userAndOrg);

        void NotifyAboutNewUser(ApplicationUser newUser, int orgId);

        void SendUserResetPasswordEmail(ApplicationUser user, string token, string organizationName);

        void SendUserVerificationEmail(ApplicationUser user, string token, string organizationName);
    }
}
