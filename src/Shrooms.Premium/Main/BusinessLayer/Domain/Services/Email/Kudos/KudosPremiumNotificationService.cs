using System.Data.Entity;
using System.Linq;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Kudos
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
