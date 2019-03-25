using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.EntityValidationValues;

namespace Shrooms.WebViewModels.Models.Wall.Posts.Comments
{
    public class EditCommentViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(ValidationConstants.MaxCommentMessageBodyLength)]
        public string MessageBody { get; set; }

        public string PictureId { get; set; }
    }
}
