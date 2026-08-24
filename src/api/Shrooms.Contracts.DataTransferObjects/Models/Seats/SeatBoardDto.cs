using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Seats
{
    public class SeatBoardDto
    {
        public int FloorId { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public IEnumerable<SeatDto> Seats { get; set; }

        public IEnumerable<SeatReservationDto> Reservations { get; set; }

        public IEnumerable<SeatReleaseDto> Releases { get; set; }

        public IEnumerable<SeatDto> MySeats { get; set; }

        public IEnumerable<SeatReservationDto> MyReservations { get; set; }

        public IEnumerable<SeatReleaseDto> MyReleases { get; set; }

        public IEnumerable<SeatHistoryDto> MyHistory { get; set; }
    }
}
