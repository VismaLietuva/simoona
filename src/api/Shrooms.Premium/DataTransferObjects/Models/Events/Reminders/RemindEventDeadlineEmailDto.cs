using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Events.Reminders
{
    public class RemindEventDeadlineEmailDto : RemindEventBaseDto
    {
        public DateTime StartDate { get; set; }

        public DateTime DeadlineDate { get; set; }
    }
}