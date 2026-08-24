using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Mentions;

namespace Shrooms.Domain.Services.Wall.Mentions
{
    public interface IMentionSearchService
    {
        /// <summary>
        /// People and taggable groups matching what has been typed after the '@'.
        /// An empty search returns the first entries so the list can open on '@'.
        /// Only groups the resolver would accept are offered, so the picker can never
        /// suggest a tag that would then notify nobody.
        /// </summary>
        Task<MentionSuggestionsDto> SearchAsync(string search, UserAndOrganizationDto userOrg);
    }
}
