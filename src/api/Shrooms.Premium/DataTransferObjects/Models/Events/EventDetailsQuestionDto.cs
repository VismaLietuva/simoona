using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventDetailsQuestionDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int Order { get; set; }

        public EventQuestionSelectType SelectType { get; set; }

        public bool IsRequired { get; set; }

        public int? ShowIfOptionId { get; set; }

        public IEnumerable<EventDetailsQuestionOptionDto> Options { get; set; }
    }
}
