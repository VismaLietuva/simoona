using System;
using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.Events
{
    public class EventChangeOptionsDTO : UserAndOrganizationDTO
    {
        public Guid EventId { get; set; }

        public IEnumerable<int> ChosenOptions { get; set; }
    }
}