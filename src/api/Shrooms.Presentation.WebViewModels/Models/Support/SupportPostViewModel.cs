using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.ViewModels;

namespace Shrooms.Presentation.WebViewModels.Models.Support
{
    public class SupportPostViewModel : AbstractViewModel
    {
        [Required]
        [StringLength(ValidationConstants.SupportSubjectMaxLength)]
        public string Subject { get; set; }

        [Required]
        [StringLength(ValidationConstants.SupportMessageBodyMaxLength)]
        public string Message { get; set; }

        [Required]
        public int Type { get; set; }
    }
}