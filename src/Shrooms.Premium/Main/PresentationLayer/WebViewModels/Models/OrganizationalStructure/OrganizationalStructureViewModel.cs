using System.Collections.Generic;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.OrganizationalStructure
{
    public class OrganizationalStructureViewModel
    {
        public string FullName { get; set; }

        public string PictureId { get; set; }

        public IEnumerable<OrganizationalStructureViewModel> Children { get; set; }
    }
}
