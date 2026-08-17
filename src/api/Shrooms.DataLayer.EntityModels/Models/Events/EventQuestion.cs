using System;
using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Events
{
    public class EventQuestion : SoftDeletableModel
    {
        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public EventQuestionSelectType SelectType { get; set; }
        public bool IsRequired { get; set; }

        /// <summary>
        /// Null means the question is always shown. Otherwise the question is shown only when
        /// this option is chosen. The referenced option always belongs to a question with a
        /// strictly lower <see cref="Order"/>, which makes cycles impossible.
        /// </summary>
        public int? ShowIfOptionId { get; set; }
        public virtual EventOption ShowIfOption { get; set; }

        public virtual ICollection<EventOption> Options { get; set; }
    }
}
