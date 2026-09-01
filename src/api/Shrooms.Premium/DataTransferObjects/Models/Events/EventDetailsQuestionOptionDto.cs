using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    /// <summary>
    /// A question's option as the details view sees it: the same id/name/order the attendee wizard
    /// gets, plus who picked it. <see cref="EventDetailsOptionDto"/> is the legacy flat food option;
    /// the two never overlap.
    /// </summary>
    public class EventDetailsQuestionOptionDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Order { get; set; }

        public IEnumerable<EventDetailsParticipantDto> Participants { get; set; }
    }
}
