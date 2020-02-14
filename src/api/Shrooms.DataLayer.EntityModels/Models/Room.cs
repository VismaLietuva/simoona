using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class Room : BaseModelWithOrg
    {
        public string Name { get; set; }

        public string Number { get; set; }

        public string Coordinates { get; set; }

        [ForeignKey("Floor")]
        public int? FloorId { get; set; }

        public virtual Floor Floor { get; set; }

        [ForeignKey("RoomType")]
        public int? RoomTypeId { get; set; }

        public virtual RoomType RoomType { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }
    }
}