using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventOptionsDto
    {
        public int MaxOptions { get; set; }
        public IEnumerable<EventOptionDto> Options { get; set; }
    }
}
