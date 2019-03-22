using System.Threading.Tasks;

namespace Shrooms.Domain.Services.WebHookCallbacks.Events
{
    public interface IEventsWebHookService
    {
        Task UpdateRecurringEvents();
    }
}