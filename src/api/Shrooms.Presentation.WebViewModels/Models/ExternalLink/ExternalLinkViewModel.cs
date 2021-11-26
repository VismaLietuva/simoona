using Shrooms.Contracts.Enums;

namespace Shrooms.Presentation.WebViewModels.Models.ExternalLink
{
    public class ExternalLinkViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
        
        public ExternalLinkTypeEnum Type { get; set; }
    }
}
