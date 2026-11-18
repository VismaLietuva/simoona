using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Search;

namespace Shrooms.Domain.Services.Search
{
    public interface IGlobalSearchService
    {
        /// <summary>
        /// People, walls, events and posts matching the search, at most <paramref name="limit"/> of each.
        /// A group the user has no permission for comes back empty, and so does every group
        /// when the search is shorter than <see cref="GlobalSearchService.MinSearchLength"/>.
        /// </summary>
        Task<GlobalSearchResultsDto> SearchAsync(string search, int limit, UserAndOrganizationDto userOrg, CancellationToken cancellationToken = default);
    }
}
