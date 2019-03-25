using System;
using Shrooms.WebViewModels.Models.User;

namespace Shrooms.WebViewModels.Models
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