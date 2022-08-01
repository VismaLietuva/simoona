using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
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
            var userEmails = await _usersDbSet
                .Include(user => user.NotificationsSettings)
                .Where(user => user.OrganizationId == organizationId)
                .Where(user => user.OrganizationId == organizationId &&
                               user.NotificationsSettings.CreatedLotteryEmailNotifications)
                .Select(user => user.Email)
                .ToListAsync();

            var organizationShortName = (await _organizationService
                .GetOrganizationByIdAsync(organizationId))
                .ShortName;

            var userNotificationSettingsUrl = _applicationSettings.UserNotificationSettingsUrl(organizationShortName);
            var feedUrl = _applicationSettings.FeedUrl(organizationShortName);

            var emailTemplateViewModel = new StartedLotteryEmailTemplateViewModel(startedDto, feedUrl, userNotificationSettingsUrl);
            var emailBody = _mailTemplate.Generate(emailTemplateViewModel, EmailPremiumTemplateCacheKeys.StartedLottery);

            var emailSubject = string.Format(Lottery.StartedLotteryEmailSubject, startedDto.Title);

            await _mailingService.SendEmailAsync(new EmailDto(userEmails, emailSubject, emailBody));
        }
    }
}
