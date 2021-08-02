using System.Threading.Tasks;

namespace Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification
{
    public interface IBirthdaysNotificationWebHookService
    {
        Task SendNotificationsAsync(string organizationName);
    }
}