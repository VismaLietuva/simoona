using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.Map
{
    public class MapRoomViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public string Number { get; set; }

        public string Coordinates { get; set; }

        public IEnumerable<MapApplicationUserViewModel> ApplicationUsers { get; set; }

        public int? RoomTypeId { get; set; }

        public MapRoomTypeViewModel RoomType { get; set; }
    }
}