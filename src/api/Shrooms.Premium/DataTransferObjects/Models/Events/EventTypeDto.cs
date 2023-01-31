namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventTypeDto
    {
        public int Id { get; set; }

        public bool IsSingleJoin { get; set; }

        public bool SendWeeklyReminders { get; set; }

        public string Name { get; set; }

        public string SingleJoinGroupName { get; set; }

        public bool IsShownWithMainEvents { get; set; }

        public bool SendEmailToManager { get; set; }

        public bool HasActiveEvents { get; set; }

        public bool IsShownInUpcomingEvents { get; set; }
    }
}
