using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.EntityValidationValues;
using Shrooms.WebViewModels.ValidationAttributes;

namespace Shrooms.WebViewModels.Models.Wall.Posts
{
    public class EditPostViewModel
    {
        [Required]
        [MinValue(1)]
        public int Id { get; set; }

        [Required]
        [StringLength(ValidationConstants.MaxCommentMessageBodyLength)]
        public string MessageBody { get; set; }

        public string PictureId { get; set; }
    }
}