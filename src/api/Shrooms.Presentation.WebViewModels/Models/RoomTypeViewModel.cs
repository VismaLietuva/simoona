using System.Web;
using Shrooms.Presentation.WebViewModels.Models.PostModels;

namespace Shrooms.Presentation.WebViewModels.Models
{
    public class RoomTypeViewModel : RoomTypePostViewModel
    {
        public HttpPostedFileBase PostedIcon { get; set; }

        public RoomTypeViewModel()
        {
            Color = "#FFFFFF";
        }
    }
}