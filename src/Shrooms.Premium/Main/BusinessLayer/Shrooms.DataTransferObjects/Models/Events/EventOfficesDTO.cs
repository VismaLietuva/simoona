using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.Models.Events
{
    public class EventOfficesDTO
    {
        public string Value { get; set; }

        public IEnumerable<string> OfficeNames { get; set; }
    }
}
