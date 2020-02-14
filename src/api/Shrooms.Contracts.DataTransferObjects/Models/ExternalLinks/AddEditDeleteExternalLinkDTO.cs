using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.ExternalLinks
{
    public class AddEditDeleteExternalLinkDTO : UserAndOrganizationDTO
    {
        public IEnumerable<ExternalLinkDTO> LinksToUpdate { get; set; }

        public IEnumerable<NewExternalLinkDTO> LinksToCreate { get; set; }

        public IEnumerable<int> LinksToDelete { get; set; }
    }
}
