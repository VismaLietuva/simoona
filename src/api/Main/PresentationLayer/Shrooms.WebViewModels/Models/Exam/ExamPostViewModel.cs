using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.Exam
{
    public class ExamPostViewModel : AbstractViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "RequiredError")]
        [StringLength(EntityModels.Models.Exam.MaxTitleLength, ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "StringLengthError")]
        public string Title { get; set; }

        [StringLength(EntityModels.Models.Exam.MaxNumberLength, ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "StringLengthError")]
        public string Number { get; set; }
    }
}