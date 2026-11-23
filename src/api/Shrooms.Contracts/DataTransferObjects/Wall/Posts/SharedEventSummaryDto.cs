using System;
using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Wall.Posts
{
    // What a wall post needs to show the event it shares, read for a whole feed page
    // at once so clients no longer call Events/Details and Events/Options per post.
    public class SharedEventSummaryDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ImageName { get; set; }

        public string Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime RegistrationDeadlineDate { get; set; }

        // In-person seats only, the same rule as Events/Details.
        public bool IsFull { get; set; }

        // The viewer is going, in person or virtually.
        public bool IsAttending { get; set; }

        // The viewer has any answer (going, maybe or not going).
        public bool HasResponse { get; set; }

        // In-person and virtual together.
        public int GoingCount { get; set; }

        // A few people who are going, without the viewer. Empty when the viewer
        // may not see the participant list.
        public IEnumerable<SharedEventAttendeeDto> Attendees { get; set; }

        // Joining needs the event page's wizard: the event asks questions, or a pick
        // from its flat options.
        public bool NeedsSignUpSteps { get; set; }
    }

    public class SharedEventAttendeeDto
    {
        public string UserId { get; set; }

        public string FullName { get; set; }

        public string ImageName { get; set; }
    }
}
