using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Committees;

namespace Shrooms.Premium.Domain.Services.Committees
{
    public interface ICommitteesService
    {
        Task PutCommitteeAsync(CommitteePostDTO modelDTO);
        Task PostCommitteeAsync(CommitteePostDTO modelDTO);
        Task<CommitteeViewDTO> GetKudosCommitteeAsync();
        Task<int> GetKudosCommitteeIdAsync();
        Task PostSuggestionAsync(CommitteeSuggestionPostDTO modelDTO, string userId);
        Task<IEnumerable<CommitteeSuggestionDto>> GetCommitteeSuggestions(int id);
        Task DeleteCommitteeSuggestion(int comitteeId, int suggestionId, UserAndOrganizationDTO userAndOrg);
    }
}
