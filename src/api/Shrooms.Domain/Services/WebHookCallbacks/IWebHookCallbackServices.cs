using Shrooms.Domain.Services.DailyMailingService;
using Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification;
using Shrooms.Domain.Services.WebHookCallbacks.BlacklistUsers;
using Shrooms.Domain.Services.WebHookCallbacks.UserAnonymization;

namespace Shrooms.Domain.Services.WebHookCallbacks
{
    public interface IWebHookCallbackServices
    {
        IBirthdaysNotificationWebHookService BirthdaysNotification { get; }

        IUsersAnonymizationWebHookService UsersAnonymization { get; }

        IDailyMailingService DailyMails { get; }

        IBlacklistUserStatusChangeWebHookService BlacklistUserStatusChange { get; }
    }
}