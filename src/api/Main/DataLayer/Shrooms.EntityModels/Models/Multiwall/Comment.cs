using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.EntityModels.Models.Multiwall
{
    public class Comment : LikeBaseModel
    {
        public const int MaxMessageBodyLength = 5000;

        [StringLength(MaxMessageBodyLength)]
        public string MessageBody { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }

        public virtual Post Post { get; set; }

        public string PictureId { get; set; }

        public DateTime LastEdit { get; set; }

        public bool IsHidden { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(MessageBody) && PictureId == null)
            {
                yield return new ValidationResult(Resources.Common.EmptyMessageError, new[] { "MessageBody", "PictureId" });
            }

            foreach (var error in base.Validate(validationContext))
            {
                yield return error;
            }
        }
    }
}
