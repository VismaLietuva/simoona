using Shrooms.DataTransferObjects.Models;
using Shrooms.EntityModels.Models.Lotteries;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Lotteries
{
    public interface ILotteryAbortJob
    {
        void RefundLottery(int lotteryId, UserAndOrganizationDTO userOrg);
    }
}
