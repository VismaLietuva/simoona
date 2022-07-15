using System;

namespace Shrooms.Contracts.DataTransferObjects.Users
{
    public class UserEventAttendStatusChangeEmailDto
    {
        public string FullName => $"{FirstName} {LastName}";

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int OrganizationId { get; set; }

        public string ManagerId { get; set; }

        public string ManagerEmail { get; set; }

        public string EventName { get; set; }

        public Guid EventId { get; set; }

        public DateTime EventStartDate { get; set; }

        public DateTime EventEndDate { get; set; }
    }
}
