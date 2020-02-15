using Shrooms.Premium.DataTransferObjects.Models.Committees;

namespace Shrooms.Premium.Domain.Services.Email.Committee
{
    public interface ICommitteeNotificationService
    {
        void NotifyCommitteeMembersAboutNewSuggestion(CommitteeSuggestionCreatedDto createdDto);
    }
}
