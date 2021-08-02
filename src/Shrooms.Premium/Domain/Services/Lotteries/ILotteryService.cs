using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.Services.Args;
using X.PagedList;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface ILotteryService
    {
        Task<Lottery> GetLotteryAsync(int lotteryId);

        Task<LotteryDTO> CreateLottery(LotteryDTO newLotteryDTO);

        Task EditDraftedLotteryAsync(LotteryDTO lotteryDTO);

        Task EditStartedLotteryAsync(EditStartedLotteryDTO lotteryDTO);

        bool AbortLottery(int lotteryId, UserAndOrganizationDTO userOrg);

        void RefundParticipants(int lotteryId, UserAndOrganizationDTO userOrg);

        Task FinishLotteryAsync(int lotteryId, UserAndOrganizationDTO userOrg);

        Task<LotteryStatsDTO> GetLotteryStatsAsync(int lotteryId, UserAndOrganizationDTO userOrg);

        Task BuyLotteryTicketAsync(BuyLotteryTicketDTO lotteryTicketDTO, UserAndOrganizationDTO userOrg);

        IEnumerable<LotteryDetailsDTO> GetLotteries(UserAndOrganizationDTO userOrganization);

        Task<List<LotteryDetailsDTO>> GetRunningLotteriesAsync(UserAndOrganizationDTO userAndOrganization);

        IEnumerable<LotteryDetailsDTO> GetFilteredLotteries(string filter, UserAndOrganizationDTO userOrg);

        Task<IPagedList<LotteryDetailsDTO>> GetPagedLotteriesAsync(GetPagedLotteriesArgs args);

        Task<LotteryDetailsDTO> GetLotteryDetailsAsync(int lotteryId, UserAndOrganizationDTO userOrg);

        LotteryStatusDTO GetLotteryStatus(int lotteryId, UserAndOrganizationDTO userOrg);

        Task UpdateRefundFailedFlagAsync(int lotteryId, bool isFailed, UserAndOrganizationDTO userOrg);
    }
}
