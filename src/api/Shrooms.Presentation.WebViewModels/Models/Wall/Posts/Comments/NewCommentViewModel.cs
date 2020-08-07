using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;
using Shrooms.Presentation.WebViewModels.ValidationAttributes;

namespace Shrooms.Presentation.WebViewModels.Models.Wall.Posts.Comments
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

        public IEnumerable<string> MentionedUserIds { get; set; }
    }
}
