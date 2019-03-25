using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models
{
    public class MapFloorViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public IEnumerable<MapRoomViewModel> Rooms { get; set; }

        public IEnumerable<MapRoomTypeViewModel> RoomTypes { get; set; }

        public string PictureId { get; set; }

        public string OrganizationName { get; set; }
    }
}