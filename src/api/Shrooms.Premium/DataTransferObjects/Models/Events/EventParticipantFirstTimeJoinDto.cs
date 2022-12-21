namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventParticipantFirstTimeJoinDto
    {
        public string Id { get; set; }

        public string FirstName { get; set;  }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string ManagerId { get; set; }

        public string ManagerEmail { get; set; }

        public int OrganizationId { get; set; }
    }
}
