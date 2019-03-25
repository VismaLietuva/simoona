using System.ComponentModel.DataAnnotations;
using Shrooms.WebViewModels.ValidationAttributes;

namespace Shrooms.WebViewModels.Models.ExternalLink
{
    public class UpdatedExternalLinkViewModel : NewExternalLinkViewModel
    {
        [Required]
        [MinValue(1)]
        public int Id { get; set; }
    }
}
