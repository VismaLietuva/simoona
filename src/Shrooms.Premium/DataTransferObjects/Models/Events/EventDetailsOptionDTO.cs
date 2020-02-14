using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventDetailsOptionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<EventDetailsParticipantDTO> Participants { get; set; }
    }
}
