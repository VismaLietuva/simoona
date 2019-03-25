namespace Shrooms.WebViewModels.Models
{
    public class MapRoomTypeViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public string IconId { get; set; }

        public string Color { get; set; }

        public MapRoomTypeViewModel()
        {
            this.Color = "#FFFFFF";
        }
    }
}