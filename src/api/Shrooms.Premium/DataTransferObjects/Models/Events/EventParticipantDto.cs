using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventParticipantDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<EventParticipantChoiceDto> Choices { get; set; }
    }
}
