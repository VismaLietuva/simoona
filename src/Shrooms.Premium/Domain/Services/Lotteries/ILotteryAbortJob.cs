using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface ILotteryAbortJob
    {
        void RefundLottery(int lotteryId, UserAndOrganizationDTO userOrg);
    }
}
