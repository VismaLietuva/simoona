using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Organizations;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Resources.Models.Lotteries;
using Shrooms.Domain.Services.Email;

namespace Shrooms.Premium.Domain.Services.Email.Lotteries
{
    public class LotteryNotificationService : NotificationServiceBase, ILotteryNotificationService
    {
        private readonly IDbSet<ApplicationUser> _usersDbSet;

        private readonly IOrganizationService _organizationService;
        private readonly IApplicationSettings _applicationSettings;

        public LotteryNotificationService(
            IMailingService mailingService,
            IMailTemplate mailTemplate,
            IUnitOfWork2 uow,
            IOrganizationService organizationService,
            IApplicationSettings applicationSettings)
            :
            base(applicationSettings, mailTemplate, mailingService)
        {
            _organizationService = organizationService;
            _applicationSettings = applicationSettings;

            _usersDbSet = uow.GetDbSet<ApplicationUser>();
        }

        public async Task NotifyUsersAboutStartedLotteryAsync(LotteryStartedEmailDto startedDto, int organizationId)
        {
            var receivers = await _usersDbSet
                .Include(user => user.NotificationsSettings)
                .Where(user => user.NotificationsSettings == null || user.NotificationsSettings.CreatedLotteryEmailNotifications)
                .Select(user => new LotteryStartedEmailUserInfoDto
                {
                    Email = user.Email,
                    TimeZoneKey = user.TimeZone
                })
                .ToListAsync();
            var organization = await _organizationService.GetOrganizationByIdAsync(organizationId);
            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organization);
            var lotteryUrl = $"{_applicationSettings.FeedUrl(organization.ShortName)}?lotteryId={startedDto.Id}";
            var subject = CreateSubject(Lottery.StartedLotteryEmailSubject, startedDto.Title);
            var lotteryTemplate = new StartedLotteryEmailTemplateViewModel(startedDto, lotteryUrl, startedDto.EndDate, userNotificationSettingsUrl);

            await SendMultipleEmailsAsync(receivers, subject, lotteryTemplate, EmailPremiumTemplateCacheKeys.StartedLottery);
        }
    }
}
