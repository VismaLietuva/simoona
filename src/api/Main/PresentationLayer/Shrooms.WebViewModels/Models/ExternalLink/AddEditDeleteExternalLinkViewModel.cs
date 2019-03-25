using System.Collections.Generic;
using Shrooms.WebViewModels.ValidationAttributes;

namespace Shrooms.WebViewModels.Models.ExternalLink
{
    public class AddEditDeleteExternalLinkViewModel
    {
        [HasNoDuplicateLinksAttribute]
        public IEnumerable<UpdatedExternalLinkViewModel> LinksToUpdate { get; set; }

        [HasNoDuplicateLinksAttribute]
        public IEnumerable<NewExternalLinkViewModel> LinksToCreate { get; set; }

        public IEnumerable<int> LinksToDelete { get; set; }
    }
}
