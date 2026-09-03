using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Events
{
    public interface IEventQuestionWriter
    {
        /// <summary>
        /// Validates an event's desired question tree without writing anything, so a caller can
        /// reject a bad payload before it commits work of its own. Pass a null
        /// <paramref name="eventId"/> when the event does not exist yet, which also asserts that
        /// the payload references nothing by id.
        /// </summary>
        Task ValidateAsync(Guid? eventId, IList<EventQuestionStructureDto> questions);

        /// <summary>
        /// Stages the full desired state of an event's question tree. Rows present in the
        /// database but absent from <paramref name="questions"/> are soft-deleted. Changes are
        /// left for the caller to save, so the whole update commits or none of it does.
        /// </summary>
        Task WriteAsync(Guid eventId, IList<EventQuestionStructureDto> questions, string userId);

        /// <summary>
        /// Same staging as <see cref="WriteAsync"/>, for an event that has not been inserted yet.
        /// The event's key is only assigned by the database, so the new rows hang off the navigation
        /// property instead of a foreign key and the whole create commits in one SaveChanges. Saving
        /// the event first and the tree second leaves a committed event with no questions whenever
        /// the second save fails.
        /// </summary>
        Task WriteForNewEventAsync(Event @event, IList<EventQuestionStructureDto> questions, string userId);
    }
}
