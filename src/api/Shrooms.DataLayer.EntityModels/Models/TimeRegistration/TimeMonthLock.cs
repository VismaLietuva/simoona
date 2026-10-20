using System;

namespace Shrooms.DataLayer.EntityModels.Models.TimeRegistration
{
    public class TimeMonthLock : BaseModelWithOrg
    {
        public DateTime Month { get; set; }

        public string LockedById { get; set; }

        public virtual ApplicationUser LockedBy { get; set; }
    }
}
