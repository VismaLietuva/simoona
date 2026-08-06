using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.DataTransferObjects.Models.Groups
{
    public class GroupDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public int GroupTypeId { get; set; }

        public string GroupTypeName { get; set; }

        public GroupTypeDto GroupType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public GroupStatus Status { get; set; }

        public bool IsPending { get; set; }

        /// <summary>
        /// Lets the client show the withdraw action to whoever raised a pending request.
        /// </summary>
        public string CreatedBy { get; set; }

        public string ApprovalAnswers { get; set; }

        public bool IsExpired { get; set; }

        public ICollection<GroupMemberDto> Members { get; set; }

        public ICollection<GroupReferenceDto> References { get; set; }
    }
}
