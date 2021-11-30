using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks
{
    public class ExternalLinkDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public ExternalLinkTypeEnum Type { get; set; }
    }
}
