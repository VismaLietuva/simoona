using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Committees;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Email.Committee
{
    public interface ICommitteeNotificationService
    {
        void NotifyCommitteeMembersAboutNewSuggestion(CommitteeSuggestionCreatedDto createdDto);
    }
}
