using Shrooms.Contracts.ViewModels;

namespace Shrooms.Premium.Presentation.WebViewModels.Map
{
    public class MapRoomTypeViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public string IconId { get; set; }

        public string Color { get; set; }

        public MapRoomTypeViewModel()
        {
            Color = "#FFFFFF";
        }
    }
}