using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Events.Reminders
{
    public class RemindEventStartEmailDto : RemindEventBaseDto
    {
        public DateTime StartDate { get; set; }
    }
}