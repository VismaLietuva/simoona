using System;
using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events
{
    public class EventChangeOptionsDTO : UserAndOrganizationDTO
    {
        public Guid EventId { get; set; }

        public IEnumerable<int> ChosenOptions { get; set; }
    }
}