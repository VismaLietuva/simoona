using System.Collections.Generic;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.User;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Committees
{
    public class CommitteeViewModel : AbstractViewModel
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