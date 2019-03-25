using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models
{
    public class CertificateMiniViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public bool InProgress { get; set; }

        public IEnumerable<ExamMiniViewModel> Exams { get; set; }
    }
}