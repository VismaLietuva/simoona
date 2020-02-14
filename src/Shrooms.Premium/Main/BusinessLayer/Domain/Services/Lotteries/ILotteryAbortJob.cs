using Shrooms.Contracts.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Lotteries
{
    public interface ILotteryAbortJob
    {
        void RefundLottery(int lotteryId, UserAndOrganizationDTO userOrg);
    }
}
