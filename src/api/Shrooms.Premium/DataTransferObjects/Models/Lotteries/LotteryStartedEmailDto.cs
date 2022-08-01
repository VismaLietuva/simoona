using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class LotteryStartedEmailDto
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public int EntryFee { get; set; }

        public DateTime EndDate { get; set; }
    }
}
