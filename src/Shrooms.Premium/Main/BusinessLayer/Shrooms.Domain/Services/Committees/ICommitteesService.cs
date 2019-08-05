using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Committees;

namespace Shrooms.Domain.Services.Committees
{
    public interface ICommitteesService
    {
        void PutCommittee(CommitteePostDTO modelDTO);
        void PostCommittee(CommitteePostDTO modelDTO);
        CommitteeViewDTO GetKudosCommittee();
        int GetKudosCommitteeId();
        ComiteeSuggestionCreatedDto PostSuggestion(CommitteeSuggestionPostDTO modelDTO, string userId);
        IEnumerable<CommitteeSuggestionViewDTO> GetCommitteeSuggestions(int id);
        void DeleteComitteeSuggestion(int comitteeId, int suggestionId, UserAndOrganizationDTO userAndOrg);
    }
}
