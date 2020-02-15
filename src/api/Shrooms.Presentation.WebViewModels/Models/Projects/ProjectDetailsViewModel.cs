using System.Collections.Generic;
using Shrooms.Contracts.ViewModels.User;

namespace Shrooms.Presentation.WebViewModels.Models.Projects
{
    public class ProjectDetailsViewModel
    {
        public string Name { get; set; }

        public string Desc { get; set; }

        public string LogoId { get; set; }

        public virtual ApplicationUserMinimalViewModel Owner { get; set; }

        public virtual IEnumerable<ApplicationUserMinimalViewModel> Members { get; set; }

        public virtual IEnumerable<string> Attributes { get; set; }

        public int WallId { get; set; }
    }
}
