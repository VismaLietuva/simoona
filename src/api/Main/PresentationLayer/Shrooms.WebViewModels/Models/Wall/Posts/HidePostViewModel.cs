using System.ComponentModel.DataAnnotations;
using Shrooms.WebViewModels.ValidationAttributes;

namespace Shrooms.WebViewModels.Models.Wall.Posts
{
    public class HidePostViewModel
    {
        [Required]
        [MinValue(1)]
        public int Id { get; set; }
    }
}
