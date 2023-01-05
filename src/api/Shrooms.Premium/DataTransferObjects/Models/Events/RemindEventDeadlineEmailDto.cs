using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class RemindEventDeadlineEmailDto
    {
        public string UserEmail { get; set; }

        public string EventName { get; set; }

        public DateTime DeadlineDate { get; set; }

        public string EventUrl { get; set; }
    }
}
