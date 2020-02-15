using System.Collections.Generic;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Presentation.WebViewModels.Models
{
    public class AbstractClassifierViewModel : AbstractClassifierAbstractViewModel
    {
    }

    public class AbstractClassifierViewPagedModel : PagedViewModel<AbstractClassifierViewModel>
    {
        public Organization Organization { get; set; }

        public virtual ICollection<AbstractClassifier> Children { get; set; }
    }
}