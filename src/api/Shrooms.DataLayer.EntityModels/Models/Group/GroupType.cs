using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Kudos;

namespace Shrooms.DataLayer.EntityModels.Models.Group
{
    public class GroupType : SoftDeletableModelWithOrg
    {
        public string Name { get; set; }

        /// <summary>
        /// Position in the group types list and in the groups page headings.
        /// Ties fall back to the name.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Time-bound: enables and requires start/end dates, expired groups move to "Past groups",
        /// and the type may not receive kudos.
        /// </summary>
        public bool IsTemporary { get; set; }

        public GroupCreationPolicy CreationPolicy { get; set; }

        /// <summary>
        /// Template shown to whoever creates a group of this type when it needs approval.
        /// Their filled-in copy is stored on the group as ApprovalAnswers.
        /// </summary>
        public string ApprovalQuestions { get; set; }

        /// <summary>
        /// Groups of this type can be tagged in posts. The group's Name is the handle.
        /// </summary>
        public bool HasGroupTag { get; set; }

        /// <summary>
        /// The kudos type members of these groups receive each month. Null means the
        /// type does not receive kudos at all.
        /// </summary>
        [ForeignKey("KudosType")]
        public int? KudosTypeId { get; set; }

        public virtual KudosType KudosType { get; set; }

        [NotMapped]
        public bool ReceivesKudos => KudosTypeId != null;

        public virtual ICollection<Group> Groups { get; set; }
    }
}
