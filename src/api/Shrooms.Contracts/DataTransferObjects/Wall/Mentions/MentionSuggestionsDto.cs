using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Wall.Mentions
{
    public class MentionSuggestionsDto
    {
        public IEnumerable<MentionPersonDto> People { get; set; }

        public IEnumerable<MentionGroupDto> Groups { get; set; }
    }
}
