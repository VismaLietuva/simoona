using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Groups
{
    public class GroupMemberPostDto
    {
        /// <summary>
        /// The membership row being edited, or null when adding someone.
        /// </summary>
        public int? MembershipId { get; set; }

        public string Id { get; set; }

        public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
