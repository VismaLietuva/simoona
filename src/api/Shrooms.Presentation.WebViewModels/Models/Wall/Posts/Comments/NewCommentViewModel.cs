using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;
using Shrooms.Presentation.WebViewModels.ValidationAttributes;
using Shrooms.Presentation.WebViewModels.ValidationAttributes.Walls;

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

        [HasImageOrMessageBody(nameof(MessageBody))]
        public IEnumerable<string> Images { get; set; }

        public IEnumerable<string> MentionedUserIds { get; set; }
    }
}
