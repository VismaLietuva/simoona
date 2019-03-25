using System;
using Shrooms.EntityModels.Models;

namespace DataLayer.Models
{
    public class WorkingHours : BaseModelWithOrg
    {
        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public TimeSpan? LunchStart { get; set; }

        public TimeSpan? LunchEnd { get; set; }

        public bool FullTime { get; set; }

        public int? PartTimeHours { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}