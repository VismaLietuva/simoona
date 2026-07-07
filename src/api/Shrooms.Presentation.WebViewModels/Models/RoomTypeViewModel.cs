using Microsoft.AspNetCore.Http;
using Shrooms.Presentation.WebViewModels.Models.PostModels;

namespace Shrooms.Presentation.WebViewModels.Models
{
    public class RoomTypeViewModel : RoomTypePostViewModel
    {
        public IFormFile PostedIcon { get; set; }

        public RoomTypeViewModel()
        {
            Color = "#FFFFFF";
        }
    }
}