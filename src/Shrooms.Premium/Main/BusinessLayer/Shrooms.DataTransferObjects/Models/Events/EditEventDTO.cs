using System;
using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.Events
{
    public class EditEventDTO : CreateEventDto
    {
        public Guid Id { get; set; }
        public IEnumerable<EventOptionDTO> EditedOptions { get; set; }
    }
}
