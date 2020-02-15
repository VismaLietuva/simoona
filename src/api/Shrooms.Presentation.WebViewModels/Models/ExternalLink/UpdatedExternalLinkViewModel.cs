using System.ComponentModel.DataAnnotations;
using Shrooms.Presentation.WebViewModels.ValidationAttributes;

namespace Shrooms.Presentation.WebViewModels.Models.ExternalLink
{
    public class UpdatedExternalLinkViewModel : NewExternalLinkViewModel
    {
        [Required]
        [MinValue(1)]
        public int Id { get; set; }
    }
}
