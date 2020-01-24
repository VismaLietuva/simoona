using System.Collections.Generic;

namespace Shrooms.EntityModels.Models.Committee
{
    public class Committee : BaseModelWithOrg
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public string Website { get; set; }

        public bool IsKudosCommittee { get; set; }

        public virtual ICollection<ApplicationUser> Members { get; set; }

        public virtual ICollection<CommitteeSuggestion> Suggestions { get; set; }

        public virtual ICollection<ApplicationUser> Leads { get; set; }

        public virtual ICollection<ApplicationUser> Delegates { get; set; }
    }
}
