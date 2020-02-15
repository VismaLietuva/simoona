using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.User
{
    public class ApplicationUserPutOfficeInfoViewModel : ApplicationUserBaseViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "RequiredError")]
        public int RoomId { get; set; }
    }
}