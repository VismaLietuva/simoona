using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Domain.Services.Email.Kudos
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

        private async Task SendLoyaltyBotNotificationAsync(AwardedKudosEmployeeDto kudosLog)
        {
            var organization = await GetOrganizationAsync(kudosLog.OrganizationId);
            var employee = await _employeeDbSet.SingleAsync(s => s.Id == kudosLog.EmployeeId);
            var userNotificationSettingsUrl = _appSettings.UserNotificationSettingsUrl(organization.ShortName);
            var kudosProfileUrl = _appSettings.KudosProfileUrl(organization.ShortName, kudosLog.EmployeeId);
            var subject = Resources.Models.Kudos.Kudos.EmailSubject;

            var emailTemplateViewModel = new LoyaltyKudosReceivedEmailTemplateViewModel(userNotificationSettingsUrl,
                kudosLog.Points,
                kudosLog.KudosTypeName,
                organization.Name,
                kudosLog.Comments,
                kudosProfileUrl);

            var body = _mailTemplate.Generate(emailTemplateViewModel);

            await _mailingService.SendEmailAsync(new EmailDto(employee.Email, subject, body));
        }

        private async Task<Organization> GetOrganizationAsync(int orgId)
        {
            return await _organizationsDbSet.SingleAsync(x => x.Id == orgId);
        }

        public async Task SendLoyaltyBotNotificationAsync(IEnumerable<AwardedKudosEmployeeDto> awardedEmployees)
        {
            foreach (var employee in awardedEmployees)
            {
                await SendLoyaltyBotNotificationAsync(employee);
            }
        }
    }
}
