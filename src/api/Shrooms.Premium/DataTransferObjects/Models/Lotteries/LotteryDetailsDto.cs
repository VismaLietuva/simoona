using System;
using JetBrains.Annotations;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Premium.DataTransferObjects.Models.Lotteries
{
    public class LotteryDetailsDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime EndDate { get; set; }

        public LotteryStatus Status { get; set; }

        public int EntryFee { get; set; }

        public ImageCollection Images { get; set; }

        public int Participants { get; set; }

        public bool RefundFailed { get; set; }

        public int GiftedTicketLimit { get; set; }

        [CanBeNull]
        public LotteryDetailsBuyerDto Buyer { get; set; }
    }
}
