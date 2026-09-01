using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    /// <summary>
    /// The sign-up question tree with answers, for the host-facing details view. Separate from
    /// <see cref="EventQuestionStructureDto"/> (the write shape) and from the wizard's read shape,
    /// which must not carry participants.
    /// </summary>
    public class EventDetailsQuestionDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int Order { get; set; }

        public EventQuestionSelectType SelectType { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>Null means the question is always shown.</summary>
        public int? ShowIfOptionId { get; set; }

        public IEnumerable<EventDetailsQuestionOptionDto> Options { get; set; }
    }
}
