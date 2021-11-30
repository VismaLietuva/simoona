using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
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
        [DataAnnotationsExtensions.Url(UrlOptions.RequireProtocol)]
        public string Url { get; set; }

        [Required]
        [EnumDataType(typeof(ExternalLinkTypeEnum))]
        public ExternalLinkTypeEnum Type { get; set; }
    }
}
