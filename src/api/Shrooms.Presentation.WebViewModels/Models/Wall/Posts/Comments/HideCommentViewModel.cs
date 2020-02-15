using System.ComponentModel.DataAnnotations;
using Shrooms.Presentation.WebViewModels.ValidationAttributes;

namespace Shrooms.Presentation.WebViewModels.Models.Wall.Posts.Comments
{
    public class HideCommentViewModel
    {
        [Required]
        [MinValue(1)]
        public int Id { get; set; }
    }
}
