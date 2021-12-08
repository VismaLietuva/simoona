using Shrooms.Domain.Services.DailyMailingService;
using Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification;
using Shrooms.Domain.Services.WebHookCallbacks.UserAnonymization;

namespace Shrooms.Domain.Services.WebHookCallbacks
{
    public interface IWebHookCallbackServices
    {
        IBirthdaysNotificationWebHookService BirthdaysNotification { get; set; }

        IUsersAnonymizationWebHookService UsersAnonymization { get; set; }

        IDailyMailingService DailyMails { get; set; }
    }
}