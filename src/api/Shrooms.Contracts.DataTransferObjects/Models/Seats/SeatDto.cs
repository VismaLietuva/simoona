namespace Shrooms.Contracts.DataTransferObjects.Models.Seats
{
    public class SeatDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int RoomId { get; set; }

        public int? FloorId { get; set; }

        public SeatPersonDto Owner { get; set; }
    }
}
