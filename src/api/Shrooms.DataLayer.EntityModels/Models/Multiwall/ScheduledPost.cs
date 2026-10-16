using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Contracts.Constants;

namespace Shrooms.DataLayer.EntityModels.Models.Multiwall
{
    public class ScheduledPost : BaseModelWithOrg
    {
        public const int MaxJobIdLength = 100;

        [StringLength(ValidationConstants.MaxPostMessageBodyLength)]
        public string MessageBody { get; set; }

        public ImageCollection Images { get; set; }

        public DateTime ScheduledFor { get; set; }

        public string JobId { get; set; }

        public string AuthorId { get; set; }

        [ForeignKey(nameof(AuthorId))]
        public virtual ApplicationUser Author { get; set; }

        public int WallId { get; set; }

        [ForeignKey(nameof(WallId))]
        public virtual Wall Wall { get; set; }

        public Guid Version { get; set; }
    }
}
