using System;

namespace Shrooms.Contracts.DataTransferObjects.Models.Events
{
    public class UpcomingEventWidgetDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime RegistrationDeadlineDate { get; set; }

        public string TypeName { get; set; }
    }
}