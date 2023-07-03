using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Contracts.Constants;

namespace Shrooms.DataLayer.EntityModels.Models.Multiwall
{
    public class Comment : LikeBaseModel
    {
        [StringLength(ValidationConstants.MaxCommentMessageBodyLength)]
        public string MessageBody { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }

        public virtual Post Post { get; set; }

        public ImageCollection Images { get; set; }

        public DateTime LastEdit { get; set; }

        public bool IsHidden { get; set; }
    }
}
