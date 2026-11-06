using System;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.Events.Participation
{
    /// <summary>
    /// Serializes changes to one event's seats across API instances. The static semaphore in
    /// EventParticipationService only protects a single process.
    /// </summary>
    public interface IEventSeatLock
    {
        Task RunAsync(Guid eventId, Func<Task> work);
    }
}
