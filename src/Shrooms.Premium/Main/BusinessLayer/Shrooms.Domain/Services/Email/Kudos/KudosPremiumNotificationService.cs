using Shrooms.Constants;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.DataTransferObjects.Models.Emails;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Infrastructure.Configuration;
using Shrooms.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templating;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Kudos;
using System.Collections.Generic;
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
        private readonly IDbSet<ApplicationUser> _employeeDbSet;

        public KudosPremiumNotificationService(IUnitOfWork2 uow, IMailingService mailingService, IApplicationSettings appSettings, IMailTemplate mailTemplate)
        {
            _appSettings = appSettings;
            _mailTemplate = mailTemplate;
            _mailingService = mailingService;

            _organizationsDbSet = uow.GetDbSet<Organization>();
            _employeeDbSet = uow.GetDbSet<ApplicationUser>();
        }

        private void SendLoyaltyBotNotification(AwardedKudosEmployeeDTO kudosLog)
        {
            var organization = GetOrganization(kudosLog.OrganizationId);
            var employee = _employeeDbSet.Single(s => s.Id == kudosLog.EmployeeId);
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

            _mailingService.SendEmail(new EmailDto(employee.Email, subject, body));
        }

        private Organization GetOrganization(int orgId)
        {
            return _organizationsDbSet.Single(x => x.Id == orgId);
        }

        public void SendLoyaltyBotNotification(IEnumerable<AwardedKudosEmployeeDTO> awardedEmployees)
        {
            foreach (var employee in awardedEmployees)
            {
                SendLoyaltyBotNotification(employee);
            }
        }
    }
}
