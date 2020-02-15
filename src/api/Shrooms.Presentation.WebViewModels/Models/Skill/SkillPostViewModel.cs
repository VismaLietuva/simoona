using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.Skill
{
    public class SkillPostViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "RequiredError")]
        public string Title { get; set; }
    }
}