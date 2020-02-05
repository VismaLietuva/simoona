namespace Shrooms.DataTransferObjects.Models.Events
{
    public class UpdateEventTypeDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }
        public bool IsSingleJoin { get; set; }
        public bool SendWeeklyReminders { get; set; }
        public string Name { get; set; }
        public string SingleJoinGroupName { get; set; }
        public bool IsShownWithAllEvents { get; set; }
    }
}
