namespace Shrooms.Contracts.DataTransferObjects.Models.Seats
{
    public class SeatReservationDto
    {
        public int SeatId { get; set; }

        public string Day { get; set; }

        public SeatPersonDto User { get; set; }
    }
}
