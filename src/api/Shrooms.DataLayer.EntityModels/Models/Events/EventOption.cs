using System;
using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Events
{
    public class EventOption : SoftDeletableModel
    {
        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }
        public string Option { get; set; }
        public OptionRules Rule { get; set; }

        /// <summary>
        /// Null means this is a legacy flat option, capped by <c>Event.MaxChoices</c>.
        /// </summary>
        public int? QuestionId { get; set; }
        public virtual EventQuestion Question { get; set; }

        public int Order { get; set; }

        public virtual ICollection<EventParticipant> EventParticipants { get; set; }
    }
}
