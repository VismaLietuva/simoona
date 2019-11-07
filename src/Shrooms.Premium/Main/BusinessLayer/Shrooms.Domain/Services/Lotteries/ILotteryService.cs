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

        bool AbortLottery(int id, UserAndOrganizationDTO userOrg);

        void RefundParticipants(int id, UserAndOrganizationDTO userOrg);

        Task FinishLotteryAsync(int lotteryId);

        LotteryStatsDTO GetLotteryStats (int lotteryId);

        Task BuyLotteryTicketAsync(BuyLotteryTicketDTO lotteryTicketDTO, UserAndOrganizationDTO userOrg);

        IEnumerable<LotteryDetailsDTO> GetLotteries(UserAndOrganizationDTO userOrganization);

        IEnumerable<LotteryDetailsDTO> GetRunningLotteries(UserAndOrganizationDTO userAndOrganization);

        IEnumerable<LotteryDetailsDTO> GetFilteredLotteries(string filter, UserAndOrganizationDTO userOrg);

        IPagedList<LotteryDetailsDTO> GetPagedLotteries(GetPagedLotteriesArgs args);

        LotteryDetailsDTO GetLotteryDetails(int id);

        LotteryStatusDTO GetLotteryStatus(int id);

        void UpdateRefundFailedFlag(int id, bool isFailed, UserAndOrganizationDTO userOrg);
    }
}
