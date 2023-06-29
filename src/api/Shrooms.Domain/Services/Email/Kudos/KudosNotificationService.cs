using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.Domain.Services.Email.Kudos
{
    public class KudosNotificationService : NotificationServiceBase, IKudosNotificationService
    {
        private readonly IApplicationSettings _appSettings;

        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<Organization> _organizationsDbSet;

        public KudosNotificationService(
            IUnitOfWork2 uow,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IMailTemplate mailTemplate)
            :
            base(appSettings, mailTemplate, mailingService)
        {
            _appSettings = appSettings;

            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationsDbSet = uow.GetDbSet<Organization>();
        }

        public async Task NotifyRejectedKudosLogSenderAsync(KudosLog kudosLog)
        {
            var emailRecipient = await _usersDbSet.SingleOrDefaultAsync(user => user.Id == kudosLog.CreatedBy);
            if (emailRecipient == null)
            {
                return;
            }

            var organization = await GetOrganizationAsync(kudosLog.OrganizationId);
            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organization);
            var kudosProfileUrl = _appSettings.KudosProfileUrl(organization.ShortName, kudosLog.CreatedBy);
            var emailTemplateViewModel = new KudosRejectedEmailTemplateViewModel(userNotificationSettingsUrl,
                kudosLog.Employee.FullName,
                kudosLog.Points,
                kudosLog.KudosTypeName,
                kudosLog.Comments,
                kudosLog.RejectionMessage,
                kudosProfileUrl);

            await SendSingleEmailAsync(
                emailRecipient.Email,
                Resources.Models.Kudos.Kudos.RejectedKudosEmailSubject,
                emailTemplateViewModel,
                EmailTemplateCacheKeys.KudosRejected);
        }

        public async Task NotifyAboutKudosSentAsync(AddKudosDto kudosDto)
        {
            var organization = await GetOrganizationAsync(kudosDto.KudosLog.OrganizationId);
            var recipient = _usersDbSet
                .Where(u => kudosDto.ReceivingUser.Id.Contains(u.Id))
                .Select(u => u.Email);

            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organization);
            var kudosProfileUrl = _appSettings.KudosProfileUrl(organization.ShortName, kudosDto.ReceivingUser.Id);
            var emailTemplateViewModel = new KudosSentEmailTemplateViewModel(userNotificationSettingsUrl,
                kudosDto.SendingUser.FullName,
                kudosDto.TotalKudosPointsInLog,
                kudosDto.KudosLog.Comment,
                kudosProfileUrl);

            await SendMultipleEmailsAsync(
                recipient,
                Resources.Models.Kudos.Kudos.EmailSubject,
                emailTemplateViewModel,
                EmailTemplateCacheKeys.KudosSent);
        }

        public async Task NotifyApprovedKudosRecipientAsync(KudosLog kudosLog)
        {
            var organization = await GetOrganizationAsync(kudosLog.OrganizationId);
            var sendingUserFullName = await GetUserFullNameAsync(kudosLog.CreatedBy);
            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organization);
            var kudosProfileUrl = _appSettings.KudosProfileUrl(organization.ShortName, kudosLog.EmployeeId);

            var emailTemplateViewModel = new KudosReceivedDecreasedEmailTemplateViewModel(userNotificationSettingsUrl,
                kudosLog.Points,
                kudosLog.KudosTypeName,
                sendingUserFullName ?? BusinessLayerConstants.DeletedUserName,
                kudosLog.Comments,
                kudosProfileUrl);

            await SendSingleEmailAsync(
                kudosLog.Employee.Email,
                Resources.Models.Kudos.Kudos.EmailSubject,
                emailTemplateViewModel,
                EmailTemplateCacheKeys.KudosReceived);
        }

        public async Task NotifyApprovedKudosDecreaseRecipientAsync(KudosLog kudosLog)
        {
            var organization = await GetOrganizationAsync(kudosLog.OrganizationId);
            var sendingUserFullName = await GetUserFullNameAsync(kudosLog.CreatedBy);
            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organization);
            var kudosProfileUrl = _appSettings.KudosProfileUrl(organization.ShortName, kudosLog.EmployeeId);

            var emailTemplateViewModel = new KudosReceivedDecreasedEmailTemplateViewModel(userNotificationSettingsUrl,
                kudosLog.Points,
                kudosLog.KudosTypeName,
                sendingUserFullName ?? BusinessLayerConstants.DeletedUserName,
                kudosLog.Comments,
                kudosProfileUrl);

            await SendSingleEmailAsync(
                kudosLog.Employee.Email,
                Resources.Models.Kudos.Kudos.MinusKudosEmailSubject,
                emailTemplateViewModel,
                EmailTemplateCacheKeys.KudosDecreased);
        }

        private async Task<Organization> GetOrganizationAsync(int orgId)
        {
            return await _organizationsDbSet.SingleAsync(x => x.Id == orgId);
        }

        private async Task<string> GetUserFullNameAsync(string userId)
        {
            return await _usersDbSet
                .Where(x => x.Id == userId)
                .Select(x => x.FirstName + " " + x.LastName)
                .SingleOrDefaultAsync();
        }
    }
}
