using System;

namespace Shrooms.DataLayer.EntityModels.Models.TimeRegistration
{
    public class TimeEntry : BaseModelWithOrg
    {
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public int ProjectId { get; set; }

        public virtual TimeProject Project { get; set; }

        public int TaskId { get; set; }

        public virtual TimeTask Task { get; set; }

        public DateTime Date { get; set; }

        public int Minutes { get; set; }
    }
}
