using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Shrooms.Presentation.WebViewModels.Models.PostModels
{
    public class RoomTypePostViewModel : AbstractViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "MessageRequiredField")]
        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        public string IconId { get; set; }

        [StringLength(7, MinimumLength = 7)]
        public string Color { get; set; }

        public bool IsWorkingRoom { get; set; }

        // not to use room assignment in roomtype editor
        //public IEnumerable<AbstractViewModel> Rooms { get; set; }
    }
}