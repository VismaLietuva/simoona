namespace Shrooms.Contracts.DataTransferObjects.Models.Seats
{
    public class SeatSaveArgsDto : UserAndOrganizationDto
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public string OwnerId { get; set; }
    }
}
