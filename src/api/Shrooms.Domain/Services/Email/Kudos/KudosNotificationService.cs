using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.Domain.Services.Email.Kudos
{
    public class KudosNotificationService : IKudosNotificationService
    {
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;

        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<Organization> _organizationsDbSet;

        public KudosNotificationService(IUnitOfWork2 uow, IMailingService mailingService, IApplicationSettings appSettings, IMailTemplate mailTemplate)
        {
            _appSettings = appSettings;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;

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

            var organizationName = (await GetOrganizationNameAsync(kudosLog.OrganizationId)).ShortName;
            var subject = Resources.Models.Kudos.Kudos.RejectedKudosEmailSubject;
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var kudosProfileUrl = _appSettings.KudosProfileUrl(organizationName, kudosLog.CreatedBy);

            var emailTemplateViewModel = new KudosRejectedEmailTemplateViewModel(userNotificationSettingsUrl,
                kudosLog.Employee.FullName,
                kudosLog.Points,
                kudosLog.KudosTypeName,
                kudosLog.Comments,
                kudosLog.RejectionMessage,
                kudosProfileUrl);

            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.KudosRejected);
            await _mailingService.SendEmailAsync(new EmailDto(emailRecipient.Email, subject, body));
        }

        public async Task NotifyAboutKudosSentAsync(AddKudosDTO kudosDto)
        {
            var organizationName = (await GetOrganizationNameAsync(kudosDto.KudosLog.OrganizationId)).ShortName;

            var recipient = _usersDbSet
                .Where(u => kudosDto.ReceivingUser.Id.Contains(u.Id))
                .Select(u => u.Email);

            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var kudosProfileUrl = _appSettings.KudosProfileUrl(organizationName, kudosDto.ReceivingUser.Id);
            var subject = Resources.Models.Kudos.Kudos.EmailSubject;

            var emailTemplateViewModel = new KudosSentEmailTemplateViewModel(userNotificationSettingsUrl,
                kudosDto.SendingUser.FullName,
                kudosDto.TotalKudosPointsInLog,
                kudosDto.KudosLog.Comment,
                kudosProfileUrl);

            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.KudosSent);
            await _mailingService.SendEmailAsync(new EmailDto(recipient, subject, body));
        }

        public async Task NotifyApprovedKudosRecipientAsync(KudosLog kudosLog)
        {
            var organizationName = (await GetOrganizationNameAsync(kudosLog.OrganizationId)).ShortName;
            var sendingUserFullName = await GetUserFullNameAsync(kudosLog.CreatedBy);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var kudosProfileUrl = _appSettings.KudosProfileUrl(organizationName, kudosLog.EmployeeId);
            var subject = Resources.Models.Kudos.Kudos.EmailSubject;

            var emailTemplateViewModel = new KudosReceivedDecreasedEmailTemplateViewModel(userNotificationSettingsUrl,
                kudosLog.Points,
                kudosLog.KudosTypeName,
                sendingUserFullName ?? BusinessLayerConstants.DeletedUserName,
                kudosLog.Comments,
                kudosProfileUrl);

            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.KudosReceived);
            await _mailingService.SendEmailAsync(new EmailDto(kudosLog.Employee.Email, subject, body));
        }

        public async Task NotifyApprovedKudosDecreaseRecipientAsync(KudosLog kudosLog)
        {
            var organizationName = (await GetOrganizationNameAsync(kudosLog.OrganizationId)).ShortName;
            var sendingUserFullName = await GetUserFullNameAsync(kudosLog.CreatedBy);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organizationName);
            var kudosProfileUrl = _appSettings.KudosProfileUrl(organizationName, kudosLog.EmployeeId);
            var subject = Resources.Models.Kudos.Kudos.MinusKudosEmailSubject;

            var emailTemplateViewModel = new KudosReceivedDecreasedEmailTemplateViewModel(userNotificationSettingsUrl,
                kudosLog.Points,
                kudosLog.KudosTypeName,
                sendingUserFullName ?? BusinessLayerConstants.DeletedUserName,
                kudosLog.Comments,
                kudosProfileUrl);

            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.KudosDecreased);

            await _mailingService.SendEmailAsync(new EmailDto(kudosLog.Employee.Email, subject, body));
        }

        private async Task<Organization> GetOrganizationNameAsync(int orgId)
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
