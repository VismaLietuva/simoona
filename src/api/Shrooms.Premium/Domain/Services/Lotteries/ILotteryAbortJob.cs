using Shrooms.Contracts.DataTransferObjects;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.Lotteries
{
    public interface ILotteryAbortJob
    {
        Task RefundLotteryAsync(int lotteryId, UserAndOrganizationDto userOrg);
    }
}
