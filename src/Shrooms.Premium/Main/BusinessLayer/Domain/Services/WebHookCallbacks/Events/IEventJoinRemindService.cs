namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.Events
{
    public interface IEventJoinRemindService
    {
        void SendNotifications(string orgName);
    }
}
