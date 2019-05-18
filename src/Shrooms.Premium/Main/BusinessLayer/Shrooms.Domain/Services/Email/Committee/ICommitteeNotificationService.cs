using Shrooms.EntityModels.Models.Committee;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Email.Committee
{
    public interface ICommitteeNotificationService
    {
        void NotifyCommitteeMembersAboutNewSuggestion(EntityModels.Models.Committee.Committee committee, CommitteeSuggestion suggestion);
    }
}
