using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Committees;

namespace Shrooms.Premium.Domain.Services.Committees
{
    public interface ICommitteesService
    {
        void PutCommittee(CommitteePostDTO modelDTO);
        void PostCommittee(CommitteePostDTO modelDTO);
        CommitteeViewDTO GetKudosCommittee();
        int GetKudosCommitteeId();
        void PostSuggestion(CommitteeSuggestionPostDTO modelDTO, string userId);
        IEnumerable<CommitteeSuggestionDto> GetCommitteeSuggestions(int id);
        void DeleteComitteeSuggestion(int comitteeId, int suggestionId, UserAndOrganizationDTO userAndOrg);
    }
}
