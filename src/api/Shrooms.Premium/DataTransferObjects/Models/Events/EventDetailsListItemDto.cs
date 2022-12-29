using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventDetailsListItemDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string TypeName { get; set; }

        public IEnumerable<string> OfficeNames { get; set; }

        public string Offices { get; set; }

        public int MaxVirtualParticipants { get; set; }

        public int MaxParticipants { get; set; }

        public int GoingCount { get; set; }

        public int VirtuallyGoingCount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsForAllOffices { get; set; }

        public IEnumerable<string> OfficeIds
        {
            get => Offices == null ? null : JsonConvert.DeserializeObject<string[]>(Offices);
            set => Offices = JsonConvert.SerializeObject(value);
        }
    }
}
