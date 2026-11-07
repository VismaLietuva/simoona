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
        /// <summary>
        /// Runs <paramref name="work"/> in one transaction under the event's lock. A call made while
        /// the same event's lock is already held joins the outer transaction.
        /// </summary>
        Task RunAsync(Guid eventId, Func<Task> work);

        /// <summary>
        /// Runs <paramref name="action"/> once the outermost <see cref="RunAsync"/> has committed, and
        /// drops it if that rolled back. Runs it at once when no lock is held.
        /// </summary>
        void AfterCommit(Action action);
    }
}
