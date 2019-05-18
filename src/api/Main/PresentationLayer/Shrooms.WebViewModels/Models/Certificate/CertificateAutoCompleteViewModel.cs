using System.Collections.Generic;
using Shrooms.WebViewModels.Models.Exam;

namespace Shrooms.WebViewModels.Models.Certificate
{
    public class CertificateAutoCompleteViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<ExamMiniViewModel> Exams { get; set; }
    }
}