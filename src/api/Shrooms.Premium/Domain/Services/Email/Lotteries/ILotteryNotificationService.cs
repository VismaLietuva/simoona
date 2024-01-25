using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.Email.Lotteries
{
    public interface ILotteryNotificationService
    {
        Task NotifyUsersAboutStartedLotteryAsync(LotteryStartedEmailDto lotteryStartedEmail, int organizationId);
        Task NotifyUsersAboutGiftedLotteryTicketsAsync(LotteryTicketGiftedEmailDto lotteryTicketGiftedEmail, int organizationId);
    }
}
