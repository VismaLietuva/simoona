using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.OrganizationalStructure
{
    public class OrganizationalStructureDto
    {
        public string Id { get; set; }

        public string FullName { get; set; }

        public string JobTitle { get; set; }

        public string PictureId { get; set; }

        public IEnumerable<OrganizationalStructureDto> Children { get; set; }
    }
}
