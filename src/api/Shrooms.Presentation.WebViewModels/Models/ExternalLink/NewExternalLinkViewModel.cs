using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Enums;

namespace Shrooms.Presentation.WebViewModels.Models.ExternalLink
{
    public class NewExternalLinkViewModel
    {
        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        [Required]
        [MaxLength(2000)]
        [Url]
        public string Url { get; set; }

        [Required]
        [EnumDataType(typeof(ExternalLinkTypeEnum))]
        public ExternalLinkTypeEnum Type { get; set; }

        [Range(0, int.MaxValue)]
        public int Priority { get; set; }
    }
}
