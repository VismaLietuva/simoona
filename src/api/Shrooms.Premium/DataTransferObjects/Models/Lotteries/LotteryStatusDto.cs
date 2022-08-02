using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class LotteryStatusDto
    {
        public LotteryStatus LotteryStatus { get; set; }

        public bool RefundFailed { get; set; }
    }
}
