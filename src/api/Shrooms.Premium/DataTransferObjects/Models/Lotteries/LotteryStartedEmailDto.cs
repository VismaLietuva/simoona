using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class LotteryStartedEmailDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int EntryFee { get; set; }

        public DateTime EndDate { get; set; }
    }
}
