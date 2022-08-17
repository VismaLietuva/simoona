using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes;
using Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes.Lotteries;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Lotteries
{
    public class CreateLotteryViewModel
    {
        [Required]
        public string Title { get; set; }

        [StringLength(ValidationConstants.LotteryDescriptionMaxLength)]
        public string Description { get; set; }

        [Required]
        [DateTimeGreaterThanPresentDate]
        public DateTime? EndDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int? EntryFee { get; set; }

        [Required]
        [ValidLotteryCreationStatus]
        public LotteryStatus Status { get; set; }

        [Required]
        public ImagesCollection Images { get; set; }

        [Range(0, int.MaxValue)]
        public int GiftedTicketLimit { get; set; }
    }
}
