using System;
using Shrooms.Contracts.ViewModels;

namespace Shrooms.Presentation.WebViewModels.Models.Exam
{
    public class ExamViewModel : AbstractViewModel
    {
        public string Title { get; set; }

        public string Number { get; set; }

        public ApplicationUserViewModel ApplicationUser { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }
    }
}