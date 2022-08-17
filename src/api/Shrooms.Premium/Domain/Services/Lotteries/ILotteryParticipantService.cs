using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface ILotteryParticipantService
    {
        Task<IList<LotteryRefundParticipantDto>> GetParticipantsGroupedByBuyerIdAsync(int lotteryId, UserAndOrganizationDto userOrg);

        Task<IList<LotteryParticipantDto>> GetParticipantsCountedAsync(int lotteryId);

        Task<IPagedList<LotteryParticipantDto>> GetPagedParticipantsAsync(int lotteryId, int page, int pageSize);
    }
}
