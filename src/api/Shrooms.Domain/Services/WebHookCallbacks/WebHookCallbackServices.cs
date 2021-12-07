using Shrooms.Domain.Services.DailyMailingService;
using Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification;
using Shrooms.Domain.Services.WebHookCallbacks.UserAnonymization;

namespace Shrooms.Domain.Services.WebHookCallbacks
{
    public class WebHookCallbackServices : IWebHookCallbackServices
    {
        public IBirthdaysNotificationWebHookService BirthdaysNotification { get; set; }

        public IDailyMailingService DailyMails { get; set; }

        public IUsersAnonymizationWebHookService UsersAnonymization { get; set; }
    }
}