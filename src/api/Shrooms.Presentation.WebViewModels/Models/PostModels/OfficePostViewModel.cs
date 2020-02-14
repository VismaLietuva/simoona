using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Shrooms.Presentation.WebViewModels.Models.PostModels
{
    public class OfficePostViewModel : AbstractViewModel
    {
        public bool IsDefault { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "MessageRequiredField")]
        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        public AddressPostViewModel Address { get; set; }
    }
}