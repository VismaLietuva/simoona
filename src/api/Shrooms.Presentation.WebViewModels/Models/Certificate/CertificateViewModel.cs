using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.Certificate
{
    public class CertificateViewModel : AbstractClassifierAbstractViewModel
    {
        public IEnumerable<ApplicationUserViewModel> ApplicationUsers { get; set; }
    }
}