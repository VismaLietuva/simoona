using System;
using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class LotteryDto
    {
        public int Id { get; set; }

        public LotteryStatus Status { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime EndDate { get; set; }

        public int EntryFee { get; set; }

        public IEnumerable<string> Images { get; set; }

        public int GiftedTicketLimit { get; set; }
    }
}
