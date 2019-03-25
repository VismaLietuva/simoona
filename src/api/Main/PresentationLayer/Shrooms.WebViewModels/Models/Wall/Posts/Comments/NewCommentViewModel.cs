using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.EntityValidationValues;
using Shrooms.WebViewModels.ValidationAttributes;

namespace Shrooms.WebViewModels.Models.Wall.Posts.Comments
{
    public class NewCommentViewModel
    {
        [Required]
        [MinValue(1)]
        public int PostId { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxCommentMessageBodyLength)]
        public string MessageBody { get; set; }

        public string PictureId { get; set; }
    }
}
