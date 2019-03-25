using System.Web;
using Shrooms.WebViewModels.Models.PostModels;

namespace Shrooms.WebViewModels.Models
{
    public class RoomTypeViewModel : RoomTypePostViewModel
    {
        public HttpPostedFileBase PostedIcon { get; set; }

        public RoomTypeViewModel()
        {
            this.Color = "#FFFFFF";
        }
    }
}