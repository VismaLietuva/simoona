using System.ComponentModel.DataAnnotations;
using Shrooms.EntityModels.Models;

namespace Shrooms.WebViewModels.Models.User
{
    public class ApplicationUserPutLoginInfoViewModel : ApplicationUserBaseViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "RequiredError")]
        public string UserName { get; set; }

        public string Password { get; set; }

        [StringLength(ApplicationUser.MaxPasswordLength, ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "StringLengthError", MinimumLength = ApplicationUser.MinPasswordLength)]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "CompareError")]
        public string NewPasswordConfirm { get; set; }
    }
}