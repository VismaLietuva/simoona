namespace Shrooms.Domain.Services.WebHookCallbacks.BirthdayNotification
{
    public interface IBirthdaysNotificationWebHookService
    {
        void SendNotifications(string organizationName);
    }
}