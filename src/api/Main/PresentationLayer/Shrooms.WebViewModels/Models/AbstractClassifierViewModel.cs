using System.Collections.Generic;

using Shrooms.EntityModels.Models;

namespace Shrooms.WebViewModels.Models
{
    public class AbstractClassifierViewModel : AbstractClassifierAbstractViewModel
    {
    }

    public class AbstractClassifierViewPagedModel : PagedViewModel<AbstractClassifierViewModel>
    {
        public Organization Organization { get; set; }

        public virtual ICollection<AbstractClassifier> Childs { get; set; }
    }
}