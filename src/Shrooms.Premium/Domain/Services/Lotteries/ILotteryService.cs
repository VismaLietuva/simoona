using System.Collections.Generic;
using System.Threading.Tasks;
using PagedList;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.Services.Args;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface ILotteryService
    {
        Task<LotteryDTO> CreateLottery(LotteryDTO newLotteryDTO);

        void EditDraftedLottery(LotteryDTO lotteryDTO);

        void EditStartedLottery(EditStartedLotteryDTO lotteryDTO);

        bool AbortLottery(int lotteryId, UserAndOrganizationDTO userOrg);

        void RefundParticipants(int lotteryId, UserAndOrganizationDTO userOrg);

        Task FinishLotteryAsync(int lotteryId, UserAndOrganizationDTO userOrg);

        LotteryStatsDTO GetLotteryStats(int lotteryId, UserAndOrganizationDTO userOrg);

        Task BuyLotteryTicketAsync(BuyLotteryTicketDTO lotteryTicketDTO, UserAndOrganizationDTO userOrg);

        IEnumerable<LotteryDetailsDTO> GetLotteries(UserAndOrganizationDTO userOrganization);

        IEnumerable<LotteryDetailsDTO> GetRunningLotteries(UserAndOrganizationDTO userAndOrganization);

        IEnumerable<LotteryDetailsDTO> GetFilteredLotteries(string filter, UserAndOrganizationDTO userOrg);

        IPagedList<LotteryDetailsDTO> GetPagedLotteries(GetPagedLotteriesArgs args);

        LotteryDetailsDTO GetLotteryDetails(int lotteryId, UserAndOrganizationDTO userOrg);

        LotteryStatusDTO GetLotteryStatus(int lotteryId, UserAndOrganizationDTO userOrg);

        void UpdateRefundFailedFlag(int lotteryId, bool isFailed, UserAndOrganizationDTO userOrg);
    }
}
