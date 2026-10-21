using System;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.TimeRegistration
{
    public class TimePeriod : BaseModelWithOrg
    {
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public DateTime Month { get; set; }

        public TimePeriodStatus Status { get; set; }

        public TimePeriodStatus? PreviousStatus { get; set; }
    }
}
