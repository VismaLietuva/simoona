using System;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.TimeRegistration
{
    public class TimeOvertimeRequest : BaseModelWithOrg
    {
        public const int MaxReasonLength = 500;

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public int MinutesPerDay { get; set; }

        public string Reason { get; set; }

        public TimeOvertimeRequestStatus Status { get; set; }

        public bool OrderedByManager { get; set; }

        public string RequestedById { get; set; }

        public virtual ApplicationUser RequestedBy { get; set; }

        public string DecidedById { get; set; }

        public virtual ApplicationUser DecidedBy { get; set; }

        public DateTime? DecidedAt { get; set; }

        public string ReviewComment { get; set; }
    }
}
