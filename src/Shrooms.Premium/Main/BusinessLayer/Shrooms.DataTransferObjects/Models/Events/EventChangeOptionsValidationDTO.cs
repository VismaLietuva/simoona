using System;
using System.Collections.Generic;
using Shrooms.EntityModels.Models.Events;

namespace Shrooms.DataTransferObjects.Models.Events
{
    public class EventChangeOptionsValidationDTO
    {
        public int MaxChoices { get; set; }

        public IList<string> Participants { get; set; }

        public IList<EventOption> EventOptions { get; set; }

        public DateTime RegistrationDeadline { get; set; }
    }
}