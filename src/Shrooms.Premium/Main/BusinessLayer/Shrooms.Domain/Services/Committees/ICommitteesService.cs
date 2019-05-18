﻿using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Committees;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Committees
{
    public interface ICommitteesService
    {
        void PutCommittee(CommitteePostDTO modelDTO);
        void PostCommittee(CommitteePostDTO modelDTO);
        CommitteeViewDTO GetKudosCommittee();
        int GetKudosCommitteeId();
        void PostSuggestion(CommitteeSuggestionPostDTO modelDTO, string userId);
        IEnumerable<CommitteeSuggestionViewDTO> GetCommitteeSuggestions(int id);
        void DeleteComitteeSuggestion(int comitteeId, int suggestionId, UserAndOrganizationDTO userAndOrg);
    }
}
