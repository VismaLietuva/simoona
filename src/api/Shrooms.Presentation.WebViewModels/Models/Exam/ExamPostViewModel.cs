using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.WebViewModels.Models.Exam
{
    public class ExamPostViewModel : AbstractViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "RequiredError")]
        [StringLength(ValidationConstants.ExamMaxTitleLength, ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "StringLengthError")]
        public string Title { get; set; }

        [StringLength(ValidationConstants.ExamMaxNumberLength, ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "StringLengthError")]
        public string Number { get; set; }
    }
}