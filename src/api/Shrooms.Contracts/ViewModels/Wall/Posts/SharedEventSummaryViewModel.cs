using System;
using System.Collections.Generic;

namespace Shrooms.Contracts.ViewModels.Wall.Posts
{
    // See SharedEventSummaryDto.
    public class SharedEventSummaryViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ImageName { get; set; }

        public string Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime RegistrationDeadlineDate { get; set; }

        public bool IsFull { get; set; }

        public bool IsAttending { get; set; }

        public bool HasResponse { get; set; }

        public int GoingCount { get; set; }

        public IEnumerable<SharedEventAttendeeViewModel> Attendees { get; set; }

        public bool NeedsSignUpSteps { get; set; }
    }

    public class SharedEventAttendeeViewModel
    {
        public string UserId { get; set; }

        public string FullName { get; set; }

        public string ImageName { get; set; }
    }
}
