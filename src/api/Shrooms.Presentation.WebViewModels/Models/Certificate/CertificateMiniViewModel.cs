using System.Collections.Generic;
using Shrooms.Presentation.WebViewModels.Models.Exam;

namespace Shrooms.Presentation.WebViewModels.Models.Certificate
{
    public class CertificateMiniViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public bool InProgress { get; set; }

        public IEnumerable<ExamMiniViewModel> Exams { get; set; }
    }
}