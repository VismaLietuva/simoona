using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models
{
    public class CertificateAutoCompleteViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<ExamMiniViewModel> Exams { get; set; }
    }
}