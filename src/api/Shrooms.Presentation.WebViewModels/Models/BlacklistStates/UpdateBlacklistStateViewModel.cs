using Shrooms.Contracts.Constants;
using Shrooms.Presentation.WebViewModels.ValidationAttributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.BlacklistStates
{
    public class UpdateBlacklistStateViewModel
    {
        [Required]
        public string UserId { get; set; }

        [DateTimeNotExpired]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddYears(WebApiConstants.DefaultBlacklistYearDuration);

        [StringLength(ValidationConstants.BlacklistStateReasonMaxLength)]
        public string Reason { get; set; }
    }
}
