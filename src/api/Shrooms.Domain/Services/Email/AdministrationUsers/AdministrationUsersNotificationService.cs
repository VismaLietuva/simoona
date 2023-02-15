using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.UserService;

namespace Shrooms.Domain.Services.Email.AdministrationUsers
{
    public class AdministrationUsersNotificationService : NotificationServiceBase, IAdministrationNotificationService
    {
        private readonly IDbSet<Organization> _organizationDbSet;
        private readonly IApplicationSettings _appSettings;
        private readonly IUserService _userService;

        public AdministrationUsersNotificationService(IUnitOfWork2 uow,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IMailTemplate mailTemplate,
            IUserService permissionService)
            :
            base(appSettings, mailTemplate, mailingService)
        {
            _organizationDbSet = uow.GetDbSet<Organization>();
            _appSettings = appSettings;
            _userService = permissionService;
        }

        public async Task SendConfirmedNotificationEmailAsync(string userEmail, UserAndOrganizationDto userAndOrg)
        {
            var organizationNameAndContent = await _organizationDbSet
                .Where(organization => organization.Id == userAndOrg.OrganizationId)
                .Select(organization => new { organization.ShortName, organization.WelcomeEmail })
                .FirstOrDefaultAsync();
            if (organizationNameAndContent == null)
            {
                return;
            }

            var mainPageUrl = _appSettings.ClientUrl;
            var userSettingsUrl = GetNotificationSettingsUrl(organizationNameAndContent.ShortName);
            var emailTemplateViewModel = new UserConfirmationEmailTemplateViewModel(userSettingsUrl, mainPageUrl, organizationNameAndContent.WelcomeEmail);

            await SendSingleEmailAsync(
                userEmail,
                Resources.Common.NewUserConfirmedNotificationEmailSubject,
                emailTemplateViewModel,
                EmailTemplateCacheKeys.UserConfirmation);
        }

        public async Task NotifyAboutNewUserAsync(ApplicationUser newUser, int orgId)
        {
            var userAdministrationEmails = await _userService.GetUserEmailsWithPermissionAsync(AdministrationPermissions.ApplicationUser, orgId);
            if (!userAdministrationEmails.Any())
            {
                return;
            }
            var organizationName = await _organizationDbSet
                .Where(organization => organization.Id == orgId)
                .Select(organization => organization.ShortName)
                .FirstOrDefaultAsync();

            var newUserProfileUrl = _appSettings.UserProfileUrl(organizationName, newUser.Id);
            var userSettingsUrl = GetNotificationSettingsUrl(organizationName);
            var emailTemplateViewModel = new NotificationAboutNewUserEmailTemplateViewModel(userSettingsUrl, newUserProfileUrl, newUser.FullName);

            await SendMultipleEmailsAsync(
                userAdministrationEmails,
                Resources.Common.NewUserConfirmEmailSubject,
                emailTemplateViewModel,
                EmailTemplateCacheKeys.NotificationAboutNewUser);
        }

        public async Task SendUserResetPasswordEmailAsync(ApplicationUser user, string token, string organizationName)
        {
            var userSettingsUrl = GetNotificationSettingsUrl(organizationName);
            var resetUrl = _appSettings.ResetPasswordUrl(organizationName, user.UserName, token);
            var resetPasswordTemplateViewModel = new ResetPasswordTemplateViewModel(user.FullName, userSettingsUrl, resetUrl);

            await SendSingleEmailAsync(
                user.Email,
                Resources.Common.UserResetPasswordEmailSubject,
                resetPasswordTemplateViewModel,
                EmailTemplateCacheKeys.ResetPassword);
        }

        public async Task SendUserVerificationEmailAsync(ApplicationUser user, string token, string organizationName)
        {
            var userSettingsUrl = GetNotificationSettingsUrl(organizationName);
            var verifyUrl = _appSettings.VerifyEmailUrl(organizationName, user.UserName, token);
            var verifyEmailTemplateViewModel = new VerifyEmailTemplateViewModel(user.FullName, userSettingsUrl, verifyUrl);

            await SendSingleEmailAsync(
                user.Email,
                Resources.Common.UserVerifyEmailSubject,
                verifyEmailTemplateViewModel,
                EmailTemplateCacheKeys.VerifyEmail);
        }
    }
}
