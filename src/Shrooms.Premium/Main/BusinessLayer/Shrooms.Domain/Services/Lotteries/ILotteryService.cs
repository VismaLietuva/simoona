using System.Collections.Generic;
using System.Threading.Tasks;
using PagedList;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Args;
using static Shrooms.Constants.BusinessLayer.ConstBusinessLayer;

namespace Shrooms.Domain.Services.Lotteries
{
    public interface ILotteryService
    {
        Task<CreateLotteryDTO> CreateLottery(CreateLotteryDTO newLotteryDTO);

        void EditDraftedLottery(EditDraftedLotteryDTO lotteryDTO);

        void EditStartedLottery(EditStartedLotteryDTO lotteryDTO);

        bool AbortLottery(int lotteryId, UserAndOrganizationDTO userOrg);

        void RefundParticipants(int lotteryId, UserAndOrganizationDTO userOrg);

        Task FinishLotteryAsync(int lotteryId, UserAndOrganizationDTO userOrg);

        LotteryStatsDTO GetLotteryStats (int lotteryId, UserAndOrganizationDTO userOrg);

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
