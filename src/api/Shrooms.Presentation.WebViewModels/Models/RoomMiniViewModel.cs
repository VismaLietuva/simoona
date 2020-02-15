using Shrooms.Contracts.ViewModels;

namespace Shrooms.Presentation.WebViewModels.Models
{
    public class RoomMiniViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public string Number { get; set; }

        public string Coordinates { get; set; }

        public int FloorId { get; set; }

        public FloorMiniViewModel Floor { get; set; }

        public int? RoomTypeId { get; set; }

        public RoomTypeMiniViewModel RoomType { get; set; }
    }
}