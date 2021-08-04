using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Email.AdministrationUsers
{
    public interface IAdministrationNotificationService
    {
        Task SendConfirmedNotificationEmailAsync(string userEmail, UserAndOrganizationDto userAndOrg);

        Task NotifyAboutNewUserAsync(ApplicationUser newUser, int orgId);

        Task SendUserResetPasswordEmailAsync(ApplicationUser user, string token, string organizationName);

        Task SendUserVerificationEmailAsync(ApplicationUser user, string token, string organizationName);
    }
}
