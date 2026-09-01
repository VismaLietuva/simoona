using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    /// <summary>
    /// A question whose IDs all exist. Both validators consume this shape, which is why it holds
    /// plain IDs rather than entities — it keeps them free of any database dependency.
    /// </summary>
    public class ResolvedEventQuestionDto
    {
        public int QuestionId { get; set; }
        public int Order { get; set; }
        public EventQuestionSelectType SelectType { get; set; }
        public bool IsRequired { get; set; }
        public int? ShowIfOptionId { get; set; }
        public IReadOnlyCollection<int> OptionIds { get; set; } = new List<int>();
    }
}
