using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks
{
    public class NewExternalLinkDto
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public ExternalLinkTypeEnum Type { get; set; }

        public int Priority { get; set; }
    }
}
