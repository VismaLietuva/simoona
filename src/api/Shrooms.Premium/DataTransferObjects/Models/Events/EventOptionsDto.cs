using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventOptionsDto
    {
        public int MaxOptions { get; set; }
        public IEnumerable<EventOptionDto> Options { get; set; }

        public IEnumerable<EventQuestionStructureDto> Questions { get; set; } = new List<EventQuestionStructureDto>();

        /// <summary>The calling user's current answers, so the wizard can prefill on reopen.</summary>
        public IEnumerable<int> MyChosenOptions { get; set; } = new List<int>();
    }
}
