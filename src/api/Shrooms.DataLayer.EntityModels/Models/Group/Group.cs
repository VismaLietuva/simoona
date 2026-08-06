using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Group
{
    public class Group : SoftDeletableModelWithOrg
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        [ForeignKey("GroupType")]
        public int GroupTypeId { get; set; }

        public virtual GroupType GroupType { get; set; }

        public GroupStatus Status { get; set; }

        /// <summary>
        /// The creator's answers to the type's ApprovalQuestions. Only set for groups
        /// of a type that requires approval.
        /// </summary>
        public string ApprovalAnswers { get; set; }

        public bool IsPending => Status == GroupStatus.Pending;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public virtual ICollection<GroupMember> Members { get; set; }

        public virtual ICollection<GroupReference> References { get; set; }

        public bool IsExpired(DateTime utcNow) => EndDate.HasValue && EndDate.Value < utcNow;
    }
}
