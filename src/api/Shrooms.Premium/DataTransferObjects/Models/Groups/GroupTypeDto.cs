using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Groups
{
    public class GroupTypeDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int SortOrder { get; set; }

        public bool IsTemporary { get; set; }

        public bool HasGroupTag { get; set; }

        public GroupCreationPolicy CreationPolicy { get; set; }

        public string ApprovalQuestions { get; set; }

        /// <summary>
        /// Null means these groups do not receive monthly kudos.
        /// </summary>
        public int? KudosTypeId { get; set; }

        public string KudosTypeName { get; set; }

        public decimal? KudosTypeValue { get; set; }

        /// <summary>
        /// Lets the admin UI warn before a flag change clears data.
        /// </summary>
        public int GroupCount { get; set; }
    }
}
