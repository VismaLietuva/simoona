namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.Events
{
    public interface IEventJoinRemindService
    {
        void SendNotifications(string orgName);
    }
}
