using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventOfficesDto
    {
        public string Value { get; set; }

        public IEnumerable<string> OfficeNames { get; set; }
    }
}
