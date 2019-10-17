using Shrooms.DataTransferObjects.Models;
using Shrooms.EntityModels.Models.Lotteries;

namespace Shrooms.Domain.Services.Lotteries
{
    public interface ILotteryAbortJob
    {
        void RefundLottery(Lottery lottery, UserAndOrganizationDTO userOrg);
    }
}
