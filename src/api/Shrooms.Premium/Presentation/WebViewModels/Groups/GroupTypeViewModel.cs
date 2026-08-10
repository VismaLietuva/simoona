using Shrooms.Contracts.Enums;
using Shrooms.Contracts.ViewModels;

namespace Shrooms.Premium.Presentation.WebViewModels.Groups
{
    public class GroupTypeViewModel : AbstractViewModel
    {
        public string Name { get; set; }

        public int SortOrder { get; set; }

        public bool IsTemporary { get; set; }

        public bool HasGroupTag { get; set; }

        public GroupCreationPolicy CreationPolicy { get; set; }

        public string ApprovalQuestions { get; set; }

        public int? KudosTypeId { get; set; }

        public string KudosTypeName { get; set; }

        public decimal? KudosTypeValue { get; set; }

        public int GroupCount { get; set; }
    }
}
