using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Web;

namespace Shrooms.Presentation.WebViewModels.Models.PostModels
{
    public class FloorPostViewModel : AbstractViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "MessageRequiredField")]
        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        public string Map { get; set; }

        public HttpPostedFileBase PostedMapPicture { get; set; }

        public int OfficeId { get; set; }

        public IEnumerable<AbstractViewModel> Rooms { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "MessageRequiredField")]
        [DataMember(IsRequired = true)]
        public string PictureId { get; set; }
    }
}