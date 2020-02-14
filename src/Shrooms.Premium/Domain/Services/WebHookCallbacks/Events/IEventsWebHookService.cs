using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.Events
{
    public interface IEventsWebHookService
    {
        Task UpdateRecurringEvents();
    }
}