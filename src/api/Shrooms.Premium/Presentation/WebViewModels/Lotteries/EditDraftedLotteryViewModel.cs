using System;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Lottery;

namespace Shrooms.Premium.Presentation.WebViewModels.Lotteries
{
    public class EditDraftedLotteryViewModel
    {
        [Required]
        public int? Id { get; set; }
        
        [Required]
        public string Title { get; set; }

        [Required]
        [EnumDataType(typeof(LotteryStatus))]
        public LotteryStatus? Status { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public DateTime? EndDate { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int? EntryFee { get; set; }
        
        [Required]
        public ImagesCollection Images { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int? GiftedTicketLimit { get; set; }
    }
}
