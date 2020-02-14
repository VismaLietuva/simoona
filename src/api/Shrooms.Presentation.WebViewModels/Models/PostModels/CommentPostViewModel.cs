using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;

namespace Shrooms.Presentation.WebViewModels.Models.PostModels
{
    public class CommentPostViewModel : AbstractViewModel
    {
        [MaxLength(ValidationConstants.MaxCommentMessageBodyLength, ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "MaxLengthError")]
        [Display(Name = "CommentMessageBody", ResourceType = typeof(Resources.Common))]
        public string MessageBody { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public int PostId { get; set; }

        public string PictureId { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(MessageBody) && PictureId == null)
            {
                yield return new ValidationResult(Resources.Common.EmptyMessageError);
            }

            foreach (var error in base.Validate(validationContext))
            {
                yield return error;
            }
        }
    }
}