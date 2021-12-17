using System.Collections.Generic;
using Shrooms.Contracts.ViewModels;
using Shrooms.Contracts.ViewModels.User;

namespace Shrooms.Premium.Presentation.WebViewModels.Committees
{
    public class CommitteePostViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public string Website { get; set; }

        public bool IsKudosCommittee { get; set; }

        public ICollection<ApplicationUserMinimalViewModel> Members { get; set; }

        public ICollection<ApplicationUserMinimalViewModel> Leads { get; set; }

        public ICollection<ApplicationUserMinimalViewModel> Delegates { get; set; }
    }
}