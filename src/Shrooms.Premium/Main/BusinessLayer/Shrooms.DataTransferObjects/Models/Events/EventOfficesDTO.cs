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
        public string Offices { get; set; }

        public IEnumerable<string> OfficeNames { get; set; }

        public IEnumerable<string> OfficeIds
        {
            get { return Offices == null ? null : JsonConvert.DeserializeObject<string[]>(Offices); }
            set { Offices = JsonConvert.SerializeObject(value); }
        }
    }
}
