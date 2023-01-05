using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class RemindJoinedEventEmailDto
    {
        public ICollection<RemindEventStartEmailDto> RemindStartEvents { get; set; }

        public ICollection<RemindEventDeadlineEmailDto> RemindDeadlineEvents { get; set; }
    }
}
