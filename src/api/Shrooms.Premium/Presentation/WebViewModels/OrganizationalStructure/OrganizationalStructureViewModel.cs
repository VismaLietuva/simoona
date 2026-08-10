using System.Collections.Generic;

namespace Shrooms.Premium.Presentation.WebViewModels.OrganizationalStructure
{
    public class OrganizationalStructureViewModel
    {
        public string Id { get; set; }

        public string FullName { get; set; }

        public string JobTitle { get; set; }

        public string PictureId { get; set; }

        public IEnumerable<OrganizationalStructureViewModel> Children { get; set; }
    }
}
