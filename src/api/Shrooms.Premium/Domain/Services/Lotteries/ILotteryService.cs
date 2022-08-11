using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface ILotteryService
    {
        Task<Lottery> GetLotteryByIdAsync(int id, UserAndOrganizationDto userOrg);

        Task CreateLotteryAsync(LotteryDto newLotteryDto, UserAndOrganizationDto userOrg);

        Task EditDraftedLotteryAsync(LotteryDto lotteryDto, UserAndOrganizationDto userOrg);

        Task EditStartedLotteryAsync(EditStartedLotteryDto lotteryDto, UserAndOrganizationDto userOrg);

        Task<bool> AbortLotteryAsync(int lotteryId, UserAndOrganizationDto userOrg);

        Task RefundParticipantsAsync(int lotteryId, UserAndOrganizationDto userOrg);

        Task FinishLotteryAsync(int lotteryId, UserAndOrganizationDto userOrg);

        Task<LotteryStatsDto> GetLotteryStatsAsync(int lotteryId, UserAndOrganizationDto userOrg);

        Task BuyLotteryTicketsAsync(BuyLotteryTicketsDto lotteryTicketDto, UserAndOrganizationDto userOrg);

        Task<IEnumerable<LotteryDetailsDto>> GetLotteriesAsync(UserAndOrganizationDto userOrganization);

        Task<List<LotteryDetailsDto>> GetRunningLotteriesAsync(UserAndOrganizationDto userAndOrganization);

        Task<IEnumerable<LotteryDetailsDto>> GetFilteredLotteriesAsync(string filter, UserAndOrganizationDto userOrg);

        Task<IPagedList<LotteryDetailsDto>> GetPagedLotteriesAsync(LotteryListingArgsDto args, UserAndOrganizationDto userOrg);

        Task<LotteryDetailsDto> GetLotteryDetailsAsync(int lotteryId, bool includeRemainingKudos, UserAndOrganizationDto userOrg);

        Task<LotteryStatusDto> GetLotteryStatusAsync(int lotteryId, UserAndOrganizationDto userOrg);

        Task UpdateRefundFailedFlagAsync(int lotteryId, bool isFailed, UserAndOrganizationDto userOrg);
    }
}
