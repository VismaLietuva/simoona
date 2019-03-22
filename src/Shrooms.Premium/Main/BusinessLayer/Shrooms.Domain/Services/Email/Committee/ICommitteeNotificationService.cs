using Shrooms.EntityModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Email.Committee
{
    public interface ICommitteeNotificationService
    {
        void NotifyCommitteeMembersAboutNewSuggestion(EntityModels.Models.Committee committee, CommitteeSuggestion suggestion);
    }
}
