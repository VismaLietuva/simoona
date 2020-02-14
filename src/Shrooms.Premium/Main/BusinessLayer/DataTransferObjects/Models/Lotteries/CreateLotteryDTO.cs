﻿using System;
using Shrooms.Contracts.DataTransferObjects.Models;
using Shrooms.DataLayer.EntityModels.Models.Lottery;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Lotteries
{
    public class LotteryDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EndDate { get; set; }
        public int EntryFee { get; set; }
        public ImagesCollection Images { get; set; }
    }
}
