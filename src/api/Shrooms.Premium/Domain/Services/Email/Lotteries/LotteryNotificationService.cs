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
using Shrooms.Infrastructure.Email.Extensions;

namespace Shrooms.Premium.Domain.Services.Email.Lotteries
{
    public class LotteryNotificationService : ILotteryNotificationService
    {
        private readonly IDbSet<ApplicationUser> _usersDbSet;

        private readonly IMailingService _mailingService;
        private readonly IMailTemplate _mailTemplate;
        private readonly IOrganizationService _organizationService;
        private readonly IApplicationSettings _applicationSettings;

        public LotteryNotificationService(
            IMailingService mailingService,
            IMailTemplate mailTemplate,
            IUnitOfWork2 uow,
            IOrganizationService organizationService,
            IApplicationSettings applicationSettings)
        {
            _mailingService = mailingService;
            _mailTemplate = mailTemplate;
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
            var organizationShortName = (await _organizationService
                .GetOrganizationByIdAsync(organizationId))
                .ShortName;
            var userNotificationSettingsUrl = _applicationSettings.UserNotificationSettingsUrl(organizationShortName);
            var lotteryUrl = $"{_applicationSettings.FeedUrl(organizationShortName)}?lotteryId={startedDto.Id}";
            var emailSubject = string.Format(Lottery.StartedLotteryEmailSubject, startedDto.Title);
            var lotteryTemplate = new StartedLotteryEmailTemplateViewModel(startedDto, lotteryUrl, startedDto.EndDate, userNotificationSettingsUrl);
            
            var receiverTimeZoneGroup = receivers.CreateTimeZoneGroup();
            var emailTimeZoneGroup = _mailTemplate.Generate(lotteryTemplate, EmailPremiumTemplateCacheKeys.StartedLottery, receiverTimeZoneGroup.GetTimeZoneKeys());
            await _mailingService.SendEmailsAsync(emailTimeZoneGroup.CreateEmails(receiverTimeZoneGroup, emailSubject));
        }
    }
}
