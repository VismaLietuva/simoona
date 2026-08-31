using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models.Seats
{
    public class SeatReservation : BaseModelWithOrg
    {
        public int SeatId { get; set; }

        [ForeignKey(nameof(SeatId))]
        public virtual Seat Seat { get; set; }

        public DateTime Day { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
