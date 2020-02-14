using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EditEventDTO : CreateEventDto
    {
        public new Guid Id { get; set; }
        public IEnumerable<EventOptionDTO> EditedOptions { get; set; }
    }
}
