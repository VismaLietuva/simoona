namespace Shrooms.Presentation.WebViewModels.Models.User
{
    public class ApplicationUserPasswordModel : ApplicationUserBaseViewModel
    {
        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }
}