using System;
using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.WebApi;

namespace Shrooms.WebViewModels.Models.User
{
    public class ApplicationUserPutPersonalInfoViewModel : ApplicationUserBaseViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "RequiredError")]
        [EmailAddress(ErrorMessage = null, ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "EmailAddressError")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "RequiredError")]
        [Phone(ErrorMessage = null, ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "PhoneError")]
        [MaxLength(ConstWebApi.MaxPhoneNumberLength)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "RequiredError")]
        public string PictureId { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "BirthdayNotSet")]
        public DateTime? BirthDay { get; set; }

        public string Bio { get; set; }
    }
}