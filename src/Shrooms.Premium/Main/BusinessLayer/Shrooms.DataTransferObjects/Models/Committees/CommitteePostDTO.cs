using System.Collections.Generic;
using Shrooms.EntityModels.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Committees
{
    public class CommitteePostDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public string Website { get; set; }

        public bool IsKudosCommittee { get; set; }

        public ICollection<ApplicationUser> Members { get; set; }

        public ICollection<ApplicationUser> Leads { get; set; }

        public ICollection<ApplicationUser> Delegates { get; set; }
    }
}
