using System;

namespace Shrooms.Contracts.DataTransferObjects.Employees
{
    public class WorkingHourslWithOutLunchDto
    {
        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }
    }
}
