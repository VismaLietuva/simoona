﻿using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using System;

namespace Shrooms.Premium.Presentation.WebViewModels.Lotteries
{
    public class LotteryViewModel
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
