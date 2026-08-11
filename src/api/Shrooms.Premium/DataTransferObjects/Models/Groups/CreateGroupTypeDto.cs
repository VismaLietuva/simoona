using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Groups
{
    public class CreateGroupTypeDto : UserAndOrganizationDto
    {
        public string Name { get; set; }

        public int SortOrder { get; set; }

        public bool IsTemporary { get; set; }

        public bool HasGroupTag { get; set; }

        public GroupCreationPolicy CreationPolicy { get; set; }

        public string ApprovalQuestions { get; set; }

        public int? KudosTypeId { get; set; }
    }
}
