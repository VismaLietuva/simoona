using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Shrooms.Presentation.WebViewModels.Models.ChangeProfile
{
    public class ChangeProfileLoginViewModel : ChangeProfileBaseModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "MessageRequiredField")]
        [DataMember(IsRequired = true)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(255, MinimumLength = 8)]
        public string NewPassword { get; set; }
    }
}