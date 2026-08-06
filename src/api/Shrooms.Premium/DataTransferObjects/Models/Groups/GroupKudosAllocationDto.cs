using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Groups
{
    public class GroupKudosAllocationDto
    {
        public string UserId { get; set; }

        /// <summary>
        /// Summed kudos type value across every kudos-receiving group the person is in.
        /// </summary>
        public decimal Amount { get; set; }

        public int KudosTypeId { get; set; }

        /// <summary>
        /// The groups the total came from, for the awarded kudos log comment.
        /// </summary>
        public ICollection<string> GroupNames { get; set; } = new List<string>();
    }
}
