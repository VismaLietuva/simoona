using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Groups
{
    public class GroupKudosAllocationDto
    {
        public string UserId { get; set; }

        public decimal Amount { get; set; }

        public int KudosTypeId { get; set; }

        public int GroupTypeId { get; set; }

        public string AwardTemplate { get; set; }

        public ICollection<string> GroupNames { get; set; } = new List<string>();

        public ICollection<string> Roles { get; set; } = new List<string>();
    }
}
