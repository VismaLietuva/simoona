using Shrooms.Domain.Services.DailyMailingService;
using Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification;

namespace Shrooms.Domain.Services.WebHookCallbacks
{
    public interface IWebHookCallbackServices
    {
        IBirthdaysNotificationWebHookService BirthdaysNotification { get; set; }

        IDailyMailingService DailyMails { get; set; }
    }
}