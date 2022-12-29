using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EditEventDto : CreateEventDto
    {
        public new Guid Id { get; set; }

        public bool ResetParticipantList { get; set; }

        public bool ResetVirtualParticipantList { get; set; }

        public IEnumerable<EventOptionDto> EditedOptions { get; set; }
    }
}
