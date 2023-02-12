using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Email;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Kudos;

namespace Shrooms.Premium.Domain.Services.Email.Kudos
{
    public class KudosPremiumNotificationService : NotificationServiceBase, IKudosPremiumNotificationService
    {
        private readonly IApplicationSettings _appSettings;

        private readonly IDbSet<Organization> _organizationsDbSet;
        private readonly IDbSet<ApplicationUser> _employeeDbSet;

        public KudosPremiumNotificationService(
            IUnitOfWork2 uow,
            IMailingService mailingService,
            IApplicationSettings appSettings,
            IMailTemplate mailTemplate)
            :
            base(appSettings, mailTemplate, mailingService)
        {
            _appSettings = appSettings;

            _organizationsDbSet = uow.GetDbSet<Organization>();
            _employeeDbSet = uow.GetDbSet<ApplicationUser>();
        }

        private async Task SendLoyaltyBotNotificationAsync(AwardedKudosEmployeeDto kudosLog)
        {
            var organization = await GetOrganizationAsync(kudosLog.OrganizationId);
            var employee = await _employeeDbSet.SingleAsync(s => s.Id == kudosLog.EmployeeId);
            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organization);
            var kudosProfileUrl = _appSettings.KudosProfileUrl(organization.ShortName, kudosLog.EmployeeId);

            var emailTemplateViewModel = new LoyaltyKudosReceivedDecreasedEmailTemplateViewModel(userNotificationSettingsUrl,
                kudosLog.Points,
                kudosLog.KudosTypeName,
                organization.Name,
                kudosLog.Comments,
                kudosProfileUrl);

            await SendSingleEmailAsync(
                employee.Email,
                Resources.Models.Kudos.Kudos.EmailSubject,
                emailTemplateViewModel,
                EmailPremiumTemplateCacheKeys.LoyaltyKudosReceived);
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
