using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models.Projects
{
    public class ProjectsListItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsCurrentUserAMember { get; set; }

        public IEnumerable<string> Attributes { get; set; }

        public IEnumerable<string> Members { get; set; }
    }
}
