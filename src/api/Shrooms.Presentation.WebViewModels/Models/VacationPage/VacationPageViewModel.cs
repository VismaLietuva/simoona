using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.VacationPage
{
    public class VacationPageViewModel
    {
        [Required]
        public string Content { get; set; }
    }
}