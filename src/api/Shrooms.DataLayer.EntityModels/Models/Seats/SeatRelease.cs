using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models.Seats
{
    public class SeatRelease : BaseModelWithOrg
    {
        public int SeatId { get; set; }

        [ForeignKey(nameof(SeatId))]
        public virtual Seat Seat { get; set; }

        public DateTime Day { get; set; }
    }
}
