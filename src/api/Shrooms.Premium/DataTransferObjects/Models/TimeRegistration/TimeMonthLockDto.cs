using System;

namespace Shrooms.Premium.DataTransferObjects.Models.TimeRegistration
{
    public class TimeMonthLockDto
    {
        public DateTime LockedAt { get; set; }

        public string LockedBy { get; set; }
    }
}
