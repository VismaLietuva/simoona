using System.Collections.Generic;

namespace Shrooms.WebViewModels.Models
{
    public class MapViewModel
    {
        public MapFloorViewModel Floor { get; set; }

        public MapOfficeViewModel Office { get; set; }

        public IEnumerable<MapOfficeViewModel> AllOffices { get; set; }
    }
}