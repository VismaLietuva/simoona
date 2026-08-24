using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Seats
{
    public class Seat : SoftDeletableModelWithOrg
    {
        public const int MaxNameLength = 50;

        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; set; }

        public SeatType Type { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int RoomId { get; set; }

        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; }

        public string OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ApplicationUser Owner { get; set; }

        public virtual ICollection<SeatReservation> Reservations { get; set; }

        public virtual ICollection<SeatRelease> Releases { get; set; }
    }
}
