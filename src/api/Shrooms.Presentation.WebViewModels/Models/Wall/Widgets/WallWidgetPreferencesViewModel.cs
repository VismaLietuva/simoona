using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.Wall.Widgets
{
    public class WallWidgetPreferencesViewModel
    {
        [StringLength(4096)]
        public string Preferences { get; set; }
    }
}
