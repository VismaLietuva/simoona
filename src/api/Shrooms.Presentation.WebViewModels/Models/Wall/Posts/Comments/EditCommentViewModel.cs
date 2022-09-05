using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;
using Shrooms.Presentation.WebViewModels.ValidationAttributes;
using Shrooms.Presentation.WebViewModels.ValidationAttributes.Walls;

namespace Shrooms.Presentation.WebViewModels.Models.Wall.Posts.Comments
{
    public class EditCommentViewModel
    {
        [MinValue(1)]
        public int Id { get; set; }

        [MaxLength(ValidationConstants.MaxCommentMessageBodyLength)]
        public string MessageBody { get; set; }

        [HasImageOrMessageBody(nameof(MessageBody), nameof(PictureId))]
        public ICollection<string> Images { get; set; }

        public IEnumerable<string> MentionedUserIds { get; set; }
       
        public string PictureId { get; set; }
    }
}
