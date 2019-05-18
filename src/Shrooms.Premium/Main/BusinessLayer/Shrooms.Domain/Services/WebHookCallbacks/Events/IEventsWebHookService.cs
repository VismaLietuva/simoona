using System.Threading.Tasks;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.WebHookCallbacks.Events
{
    public interface IEventsWebHookService
    {
        Task UpdateRecurringEvents();
    }
}