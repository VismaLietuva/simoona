using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class RemindEventStartEmailDto
    {
        public string UserEmail { get; set; }

        public string EventName { get; set; }

        public DateTime StartDate { get; set; }

        public string EventUrl { get; set; }
    }
}
