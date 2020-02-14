using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class Floor : BaseModelWithOrg
    {
        public string Name { get; set; }

        [ForeignKey("Office")]
        public int? OfficeId { get; set; }

        public virtual Office Office { get; set; }

        public virtual ICollection<Room> Rooms { get; set; }

        public string PictureId { get; set; }

        public virtual Picture Picture { get; set; }
    }
}