﻿using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events.Reminders
{
    public class RemindJoinedEventEmailDto
    {
        public IEnumerable<RemindEventStartEmailDto> RemindStartEvents { get; set; }

        public IEnumerable<RemindEventDeadlineEmailDto> RemindDeadlineEvents { get; set; }
    }
}