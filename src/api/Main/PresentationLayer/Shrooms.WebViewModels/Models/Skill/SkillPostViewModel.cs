using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models
{
    public class SkillPostViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "RequiredError")]
        public string Title { get; set; }
    }
}