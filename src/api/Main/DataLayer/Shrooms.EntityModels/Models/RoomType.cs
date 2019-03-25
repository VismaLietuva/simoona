using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.EntityModels.Models
{
    public class RoomType : BaseModelWithOrg
    {
        public const string DefaultColor = "#FFFFFF";

        public string Name { get; set; }

        public string IconId { get; set; }

        public bool IsWorkingRoom { get; set; }

        [StringLength(7, MinimumLength = 7)]
        public string Color { get; set; }

        public virtual ICollection<Room> Rooms { get; set; }
    }
}