using System;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;

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

        public ImageCollection Images { get; set; }

        public int GiftedTicketLimit { get; set; }
    }
}
