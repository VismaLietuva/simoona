using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.User
{
    public class SidebarFavoritesViewModel
    {
        [StringLength(4096)]
        public string Favorites { get; set; }
    }
}
