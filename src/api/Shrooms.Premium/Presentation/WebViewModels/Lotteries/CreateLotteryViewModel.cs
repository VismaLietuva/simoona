using System;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Lottery;

namespace Shrooms.Premium.Presentation.WebViewModels.Lotteries
{
    public class CreateLotteryViewModel
    {
        [Required]
        public int? Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        [StringLength(ValidationConstants.MaxPostMessageBodyLength)]
        public string Description { get; set; }
        
        [Required]
        public DateTime? EndDate { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int? EntryFee { get; set; }
        
        [Required]
        [EnumDataType(typeof(LotteryStatus))]
        public LotteryStatus Status { get; set; }

        [Required]
        public ImagesCollection Images { get; set; }

        [Range(1, int.MaxValue)]
        public int GiftedTicketLimit { get; set; }
    }
}
