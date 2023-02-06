using Shrooms.Contracts.Infrastructure.Email;

namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class LotteryStartedEmailUserInfoDto : IEmailReceiver
    {
        public string Email { get; set; }

        public string TimeZoneKey { get; set; }
    }
}
