using System.Collections.Generic;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events
{
    public class EventOptionsDTO
    {
        public int MaxOptions { get; set; }
        public IEnumerable<EventOptionDTO> Options { get; set; }
    }
}
