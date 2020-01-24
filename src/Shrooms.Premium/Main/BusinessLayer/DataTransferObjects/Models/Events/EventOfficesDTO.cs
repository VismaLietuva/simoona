using System.Collections.Generic;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events
{
    public class EventOfficesDTO
    {
        public string Value { get; set; }

        public IEnumerable<string> OfficeNames { get; set; }
    }
}
