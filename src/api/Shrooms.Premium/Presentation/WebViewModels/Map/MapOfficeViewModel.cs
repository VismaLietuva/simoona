using System.Collections.Generic;
using Shrooms.Contracts.ViewModels;

namespace Shrooms.Premium.Presentation.WebViewModels.Map
{
    public class MapOfficeViewModel : AbstractViewModel
    {
        public bool IsDefault { get; set; }

        public string Name { get; set; }

        public virtual IEnumerable<MapAllFloorsViewModel> Floors { get; set; }
    }
}