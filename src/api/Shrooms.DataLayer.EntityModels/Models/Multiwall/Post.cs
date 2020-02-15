using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Contracts.Constants;

namespace Shrooms.DataLayer.EntityModels.Models.Multiwall
{
    public class Post : LikeBaseModel
    {
        [StringLength(ValidationConstants.MaxPostMessageBodyLength)]
        public string MessageBody { get; set; }

        public DateTime LastActivity { get; set; }

        public DateTime LastEdit { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public string PictureId { get; set; }

        public bool IsHidden { get; set; }

        public string SharedEventId { get; set; }

        [ForeignKey("Wall")]
        public int WallId { get; set; }

        public Wall Wall { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(MessageBody) && PictureId == null)
            {
                yield return new ValidationResult("Message cannot be empty", new[] { "MessageBody", "PictureId" });
            }

            foreach (var error in base.Validate(validationContext))
            {
                yield return error;
            }
        }
    }
}
