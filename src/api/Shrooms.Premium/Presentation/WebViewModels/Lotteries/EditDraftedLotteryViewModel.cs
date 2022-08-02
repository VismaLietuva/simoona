using System;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Lottery;

namespace Shrooms.Premium.Presentation.WebViewModels.Lotteries
{
    public class EditDraftedLotteryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        [EnumDataType(typeof(LotteryStatus))]
        public LotteryStatus Status { get; set; }
        public string Description { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public int EntryFee { get; set; }
        public ImagesCollection Images { get; set; }
    }
}
