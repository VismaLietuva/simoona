using Shrooms.Constants;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templating;
using System.Data.Entity;
using System.Linq;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Email.Kudos
{
    public class KudosPremiumNotificationService : IKudosPremiumNotificationService
    {
        private readonly IMailTemplate _mailTemplate;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _appSettings;

        private readonly IDbSet<Organization> _organizationsDbSet;

        public KudosPremiumNotificationService(IUnitOfWork2 uow, IMailingService mailingService, IApplicationSettings appSettings, IMailTemplate mailTemplate)
        {
            _appSettings = appSettings;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;

            _organizationsDbSet = uow.GetDbSet<Organization>();
        }

        public void SendLoyaltyBotNotification(KudosLog kudosLog)
        {
            var organization = getOrganizationName(kudosLog.OrganizationId);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var kudosProfileUrl = _appSettings.KudosProfileUrl(organization.ShortName, kudosLog.EmployeeId);
            var subject = Resources.Models.Kudos.Kudos.EmailSubject;

            var emailTemplateViewModel = new KudosReceivedDecreasedEmailTemplateViewModel(
                userNotificationSettingsUrl,
                kudosLog.Points,
                kudosLog.KudosTypeName,
                organization.Name,
                kudosLog.Comments,
                kudosProfileUrl);
            var body = _mailTemplate.Generate(emailTemplateViewModel, EmailTemplateCacheKeys.KudosReceived);

            _mailingService.SendEmail(new EmailDto(kudosLog.Employee.Email, subject, body));
        }

        private Organization getOrganizationName(int orgId) => _organizationsDbSet
                .Single(x => x.Id == orgId);
    }
}
