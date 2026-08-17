using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.Services.Events
{
    public interface IEventQuestionWriter
    {
        /// <summary>
        /// Applies the full desired state of an event's question tree. Rows present in the
        /// database but absent from <paramref name="questions"/> are soft-deleted. The tree is
        /// validated in full before anything is written, so a rejected payload leaves no trace.
        /// </summary>
        Task WriteAsync(Guid eventId, IList<EventQuestionStructureDto> questions, string userId);
    }
}
