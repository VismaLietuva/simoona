using System.Threading.Tasks;
using Shrooms.Premium.DataTransferObjects.Models.Committees;

namespace Shrooms.Premium.Domain.Services.Email.Committee
{
    public interface ICommitteeNotificationService
    {
        Task NotifyCommitteeMembersAboutNewSuggestionAsync(CommitteeSuggestionCreatedDto createdDto);
    }
}
