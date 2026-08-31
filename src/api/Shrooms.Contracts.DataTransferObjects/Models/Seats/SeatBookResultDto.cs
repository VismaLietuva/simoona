namespace Shrooms.Contracts.DataTransferObjects.Models.Seats
{
    public class SeatBookResultDto
    {
        public int SeatId { get; set; }

        public string Day { get; set; }

        public SeatDto MovedFrom { get; set; }
    }
}
