using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;
using Shrooms.Presentation.WebViewModels.ValidationAttributes;
using Shrooms.Presentation.WebViewModels.ValidationAttributes.Walls;

namespace Shrooms.Presentation.WebViewModels.Models.Wall.Posts
{
    public class EditPostViewModel
    {
        [Required]
        [MinValue(1)]
        public int Id { get; set; }

        [Required]
        [StringLength(ValidationConstants.MaxCommentMessageBodyLength)]
        public string MessageBody { get; set; }

        [HasImageOrMessageBody(nameof(MessageBody))]
        public IEnumerable<string> Images { get; set; }
    }
}