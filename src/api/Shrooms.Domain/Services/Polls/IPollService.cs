using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Polls;

namespace Shrooms.Domain.Services.Polls
{
    public interface IPollService
    {
        Task<IEnumerable<PollListItemDto>> GetVisiblePollsAsync(UserAndOrganizationDto userOrg);

        Task<IEnumerable<PollListItemDto>> GetAllPollsAsync(UserAndOrganizationDto userOrg);

        Task<PollDto> GetPollAsync(int id, UserAndOrganizationDto userOrg, bool canManage);

        Task<PollDto> CreateAsync(CreatePollDto dto, bool canManage);

        Task UpdateAsync(UpdatePollDto dto, bool canManage);

        Task PublishAsync(PollReviewArgsDto args);

        Task RejectAsync(PollReviewArgsDto args);

        Task CloseAsync(int id, UserAndOrganizationDto userOrg);

        Task DeleteAsync(int id, UserAndOrganizationDto userOrg);

        Task VoteAsync(PollVoteDto dto);
    }
}
