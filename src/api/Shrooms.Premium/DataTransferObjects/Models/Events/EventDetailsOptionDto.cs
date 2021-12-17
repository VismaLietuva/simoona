using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventDetailsOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<EventDetailsParticipantDto> Participants { get; set; }
    }
}
