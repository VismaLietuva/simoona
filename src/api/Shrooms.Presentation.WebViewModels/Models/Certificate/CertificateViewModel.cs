using System.Collections.Generic;
using Shrooms.Presentation.WebViewModels.Models.User;

namespace Shrooms.Presentation.WebViewModels.Models.Certificate
{
    public class CertificateViewModel : AbstractClassifierAbstractViewModel
    {
        public IEnumerable<ApplicationUserViewModel> ApplicationUsers { get; set; }
    }
}