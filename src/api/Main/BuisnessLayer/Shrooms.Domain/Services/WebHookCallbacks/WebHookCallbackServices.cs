using Shrooms.Domain.Services.DailyMailingService;
using Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification;

namespace Shrooms.Domain.Services.WebHookCallbacks
{
    public class WebHookCallbackServices : IWebHookCallbackServices
    {
        public IBirthdaysNotificationWebHookService BirthdaysNotification { get; set; }

        public IDailyMailingService DailyMails { get; set; }
    }
}