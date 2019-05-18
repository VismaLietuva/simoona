using System;
using System.Collections.Generic;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Events
{
    public class EditEventDTO : CreateEventDto
    {
        public new Guid Id { get; set; }
        public IEnumerable<EventOptionDTO> EditedOptions { get; set; }
    }
}
