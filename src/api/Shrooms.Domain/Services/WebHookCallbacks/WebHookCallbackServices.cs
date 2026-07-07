using Shrooms.Domain.Services.DailyMailingService;
using Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification;
using Shrooms.Domain.Services.WebHookCallbacks.BlacklistUsers;
using Shrooms.Domain.Services.WebHookCallbacks.UserAnonymization;

namespace Shrooms.Domain.Services.WebHookCallbacks
{
    public class WebHookCallbackServices : IWebHookCallbackServices
    {
        public IBirthdaysNotificationWebHookService BirthdaysNotification { get; }

        public IUsersAnonymizationWebHookService UsersAnonymization { get; }

        public IDailyMailingService DailyMails { get; }

        public IBlacklistUserStatusChangeWebHookService BlacklistUserStatusChange { get; }

        public WebHookCallbackServices(
            IBirthdaysNotificationWebHookService birthdaysNotification,
            IUsersAnonymizationWebHookService usersAnonymization,
            IDailyMailingService dailyMails,
            IBlacklistUserStatusChangeWebHookService blacklistUserStatusChange)
        {
            BirthdaysNotification = birthdaysNotification;
            UsersAnonymization = usersAnonymization;
            DailyMails = dailyMails;
            BlacklistUserStatusChange = blacklistUserStatusChange;
        }
    }
}