using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface ILotteryAbortJob
    {
        Task RefundLotteryAsync(int lotteryId, UserAndOrganizationDTO userOrg);
    }
}
