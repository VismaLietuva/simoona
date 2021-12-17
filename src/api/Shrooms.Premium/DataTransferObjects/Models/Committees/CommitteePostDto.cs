using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.Models.Committees
{
    public class CommitteePostDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public string Website { get; set; }

        public bool IsKudosCommittee { get; set; }

        public ICollection<ApplicationUserMinimalDto> Members { get; set; }

        public ICollection<ApplicationUserMinimalDto> Leads { get; set; }

        public ICollection<ApplicationUserMinimalDto> Delegates { get; set; }
    }
}
