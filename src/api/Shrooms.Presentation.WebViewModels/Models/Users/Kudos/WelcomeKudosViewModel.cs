using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.Users.Kudos
{
    public class WelcomeKudosViewModel
    {
        [Required]
        public int WelcomeKudosAmount { get; set; }
        [Required]
        public bool WelcomeKudosIsActive { get; set; }

        public string WelcomeKudosComment { get; set; }
    }
}
