using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Presentation.WebViewModels.Groups
{
    public class CreateGroupTypeViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        public int SortOrder { get; set; }

        public bool IsTemporary { get; set; }

        public bool HasGroupTag { get; set; }

        public GroupCreationPolicy CreationPolicy { get; set; }

        public string ApprovalQuestions { get; set; }

        public int? KudosTypeId { get; set; }
    }
}
