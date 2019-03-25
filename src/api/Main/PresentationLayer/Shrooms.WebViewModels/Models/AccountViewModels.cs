using System.ComponentModel.DataAnnotations;
using Shrooms.Resources.Views.Account;

namespace Shrooms.WebViewModels.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Account), Name = "CurrentPassword")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessageResourceType = typeof(Account), ErrorMessageResourceName = "ValidationLengthError", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessageResourceType = typeof(Account), ErrorMessageResourceName = "ValidationPasswordDontMatch")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string ClientId { get; set; }

        [Required]
        public bool IsPersistance { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessageResourceType = typeof(Account), ErrorMessageResourceName = "ValidationLengthError", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessageResourceType = typeof(Account), ErrorMessageResourceName = "ValidationPasswordDontMatch")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessageResourceType = typeof(Account), ErrorMessageResourceName = "PasswordLengthInvalid", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessageResourceType = typeof(Account), ErrorMessageResourceName = "ValidationPasswordDontMatch")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class VerifyEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }
    }
}