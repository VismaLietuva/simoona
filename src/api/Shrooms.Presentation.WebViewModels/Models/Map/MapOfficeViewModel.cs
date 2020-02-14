using System.Collections.Generic;

namespace Shrooms.Presentation.WebViewModels.Models.Map
{
    public class MapOfficeViewModel : AbstractViewModel
    {
        public bool IsDefault { get; set; }

        public string Name { get; set; }

        public virtual IEnumerable<MapAllFloorsViewModel> Floors { get; set; }
    }
}