namespace Shrooms.Contracts.DataTransferObjects.Models.Seats
{
    public class SeatBoardArgsDto : UserAndOrganizationDto
    {
        public int FloorId { get; set; }

        public string From { get; set; }

        public string To { get; set; }
    }
}
