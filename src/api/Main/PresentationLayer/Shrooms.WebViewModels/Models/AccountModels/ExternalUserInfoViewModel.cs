namespace Shrooms.WebViewModels.Models.AccountModels
{
    public class ExternalUserInfoViewModel
    {
        public string Email { get; set; }

        public bool HasRegistered { get; set; }

        public string LoginProvider { get; set; }
    }
}
