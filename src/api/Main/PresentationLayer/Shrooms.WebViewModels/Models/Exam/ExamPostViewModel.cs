using System.ComponentModel.DataAnnotations;
using Shrooms.EntityModels.Models;

namespace Shrooms.WebViewModels.Models
{
    public class ExamPostViewModel : AbstractViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "RequiredError")]
        [StringLength(Exam.MaxTitleLength, ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "StringLengthError")]
        public string Title { get; set; }

        [StringLength(Exam.MaxNumberLength, ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "StringLengthError")]
        public string Number { get; set; }
    }
}