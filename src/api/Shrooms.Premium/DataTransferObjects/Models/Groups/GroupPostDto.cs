using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.Models.Groups
{
    public class GroupPostDto : UserAndOrganizationDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public int GroupTypeId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string ApprovalAnswers { get; set; }

        public ICollection<GroupMemberPostDto> Members { get; set; }

        public ICollection<GroupReferenceDto> References { get; set; }
    }
}
