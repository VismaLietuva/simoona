using System.Collections.Generic;
using Shrooms.Presentation.WebViewModels.ValidationAttributes;

namespace Shrooms.Presentation.WebViewModels.Models.ExternalLink
{
    public class ManageExternalLinkViewModel
    {
        [HasNoDuplicateLinks]
        public IEnumerable<UpdatedExternalLinkViewModel> LinksToUpdate { get; set; }

        [HasNoDuplicateLinks]
        public IEnumerable<NewExternalLinkViewModel> LinksToCreate { get; set; }

        public IEnumerable<int> LinksToDelete { get; set; }
    }
}
