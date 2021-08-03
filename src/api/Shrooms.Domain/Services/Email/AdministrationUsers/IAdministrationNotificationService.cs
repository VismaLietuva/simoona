using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Email.AdministrationUsers
{
    public interface IAdministrationNotificationService
    {
        Task SendConfirmedNotificationEmailAsync(string userEmail, UserAndOrganizationDTO userAndOrg);

        Task NotifyAboutNewUserAsync(ApplicationUser newUser, int orgId);

        void SendUserResetPasswordEmail(ApplicationUser user, string token, string organizationName);

        void SendUserVerificationEmail(ApplicationUser user, string token, string organizationName);
    }
}
