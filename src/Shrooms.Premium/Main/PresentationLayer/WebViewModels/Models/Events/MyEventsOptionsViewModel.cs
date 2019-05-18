using Shrooms.Constants.BusinessLayer;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Events
{
    public class MyEventsOptionsViewModel
    {
        public string SearchString { get; set; }

        public BusinessLayerConstants.MyEventsOptions Filter { get; set; }
    }
}
