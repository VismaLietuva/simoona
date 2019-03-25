using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.Projects
{
    public class EditProjectViewModel : NewProjectViewModel
    {
        [Required]
        public int Id { get; set; }
    }
}
