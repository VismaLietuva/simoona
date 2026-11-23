using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;

namespace Shrooms.Domain.Services.Wall
{
    // Implemented next to the event services, which own the attendance rules.
    public interface ISharedEventSummaryService
    {
        // Events the viewer cannot see, or that no longer exist, are left out of the result.
        Task<IReadOnlyDictionary<Guid, SharedEventSummaryDto>> GetSummariesAsync(
            IReadOnlyCollection<Guid> eventIds,
            UserAndOrganizationDto userOrg,
            CancellationToken cancellationToken = default);
    }
}
