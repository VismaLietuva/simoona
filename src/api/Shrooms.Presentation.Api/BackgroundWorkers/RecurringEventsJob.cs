using System.Threading.Tasks;
using Hangfire;
using JetBrains.Annotations;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.Events;

namespace Shrooms.Presentation.Api.BackgroundWorkers
{
    [UsedImplicitly]
    public class RecurringEventsJob
    {
        private readonly IEventsWebHookService _eventsWebHookService;

        public RecurringEventsJob(IEventsWebHookService eventsWebHookService)
        {
            _eventsWebHookService = eventsWebHookService;
        }

        // Overlapping runs would each clone the same expired occurrence. Hangfire reads filters off
        // the method it invokes, so the guard sits here and the domain interface stays unaware of it.
        [DisableConcurrentExecution(600)]
        public Task RollForwardExpiredOccurrencesAsync()
        {
            return _eventsWebHookService.UpdateRecurringEventsAsync();
        }
    }
}
