using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Committees;

namespace Shrooms.Premium.Domain.Services.Committees
{
    public interface ICommitteesService
    {
        Task PutCommitteeAsync(CommitteePostDto committeePostDto);
        Task PostCommitteeAsync(CommitteePostDto committeePostDto);
        Task<CommitteeViewDto> GetKudosCommitteeAsync();
        Task<int> GetKudosCommitteeIdAsync();
        Task PostSuggestionAsync(CommitteeSuggestionPostDto dto, string userId);
        Task<IEnumerable<CommitteeSuggestionDto>> GetCommitteeSuggestions(int id);
        Task DeleteCommitteeSuggestionAsync(int committeeId, int suggestionId, UserAndOrganizationDto userAndOrg);
    }
}
