using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class RemindEventStartEmailDto
    {
        public IEnumerable<string> UserEmails { get; set; }

        public Guid EventId { get; set; }

        public string EventName { get; set; }

        public DateTime StartDate { get; set; }
    }
}