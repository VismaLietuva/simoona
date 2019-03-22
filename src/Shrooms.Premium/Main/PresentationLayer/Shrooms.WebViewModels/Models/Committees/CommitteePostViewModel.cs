using Shrooms.WebViewModels.Models.User;
using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models.Committees
{
    public class CommitteePostViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public string Website { get; set; }

        public bool IsKudosCommittee { get; set; }

        public ICollection<ApplicationUserViewModel> Members { get; set; }

        public ICollection<ApplicationUserViewModel> Leads { get; set; }

        public ICollection<ApplicationUserViewModel> Delegates { get; set; }
    }
}