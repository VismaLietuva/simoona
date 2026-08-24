namespace Shrooms.Contracts.DataTransferObjects.Models.Seats
{
    public class SeatDayArgsDto : UserAndOrganizationDto
    {
        public int SeatId { get; set; }

        public string Day { get; set; }
    }
}
