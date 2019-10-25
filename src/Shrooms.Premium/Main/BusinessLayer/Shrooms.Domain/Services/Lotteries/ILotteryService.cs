using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PagedList;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Args;
using static Shrooms.Constants.BusinessLayer.ConstBusinessLayer;

namespace Shrooms.Domain.Services.Lotteries
{
    public interface ILotteryService
    {
        Task<CreateLotteryDTO> CreateLottery(CreateLotteryDTO newLotteryDTO);

        Task EditDraftedLottery(EditDraftedLotteryDTO lotteryDTO);

        Task EditStartedLottery(EditStartedLotteryDTO lotteryDTO);

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

        int GetLotteryStatus(int id);

        void EditLotteryStatus(int id, LotteryStatus status);
    }
}
