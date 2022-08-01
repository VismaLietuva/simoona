using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.Email.Lotteries
{
    public interface ILotteryNotificationService
    {
        Task NotifyUsersAboutStartedLotteryAsync(LotteryStartedEmailDto startedDto, int organizationId);
    }
}
