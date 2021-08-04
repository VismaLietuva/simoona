using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks
{
    public class AddEditDeleteExternalLinkDto : UserAndOrganizationDto
    {
        public IEnumerable<ExternalLinkDto> LinksToUpdate { get; set; }

        public IEnumerable<NewExternalLinkDto> LinksToCreate { get; set; }

        public IEnumerable<int> LinksToDelete { get; set; }
    }
}
