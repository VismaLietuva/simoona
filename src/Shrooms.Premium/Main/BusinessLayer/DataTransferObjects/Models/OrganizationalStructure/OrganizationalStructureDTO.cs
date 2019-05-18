using System.Collections.Generic;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.OrganizationalStructure
{
    public class OrganizationalStructureDTO
    {
        public string FullName { get; set; }

        public string PictureId { get; set; }

        public IEnumerable<OrganizationalStructureDTO> Children { get; set; }
    }
}
