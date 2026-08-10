using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Groups
{
    public class GroupMemberDto
    {
        /// <summary>
        /// Identifies this membership row. A person can hold several, so the user id
        /// below is not enough to tell them apart on save.
        /// </summary>
        public int MembershipId { get; set; }

        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string PictureId { get; set; }

        public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
