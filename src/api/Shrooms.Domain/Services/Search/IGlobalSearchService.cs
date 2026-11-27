using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Search;

namespace Shrooms.Domain.Services.Search
{
    public interface IGlobalSearchService
    {
        /// <summary>
        /// What matches the search in every section the user may open (people, walls, events, posts,
        /// kudos, projects, groups, books, polls, videos, service requests), at most <paramref name="limit"/> of each.
        /// A group the user has no permission for comes back empty, and so does every group
        /// when the search is shorter than <see cref="GlobalSearchService.MinSearchLength"/>.
        /// With <paramref name="sections"/> (see <see cref="GlobalSearchService.Sections"/>) only those are searched;
        /// null or empty searches them all.
        /// </summary>
        Task<GlobalSearchResultsDto> SearchAsync(string search, int limit, UserAndOrganizationDto userOrg, IReadOnlyCollection<string> sections = null, CancellationToken cancellationToken = default);
    }
}
