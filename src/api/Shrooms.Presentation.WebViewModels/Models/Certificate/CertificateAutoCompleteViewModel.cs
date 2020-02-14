using System.Collections.Generic;
using Shrooms.Presentation.WebViewModels.Models.Exam;

namespace Shrooms.Presentation.WebViewModels.Models.Certificate
{
    public class CertificateAutoCompleteViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<ExamMiniViewModel> Exams { get; set; }
    }
}