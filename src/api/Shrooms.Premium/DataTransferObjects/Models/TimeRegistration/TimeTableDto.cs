using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.TimeRegistration
{
    public class TimeTableDto
    {
        public string Month { get; set; }

        public TimeMonthLockDto Lock { get; set; }

        public int DailyMinutes { get; set; }

        public string Status { get; set; }

        public IEnumerable<TimeTableDayDto> Days { get; set; }

        public IEnumerable<TimeTableProjectDto> Projects { get; set; }

        public IEnumerable<TimeEntryDto> Entries { get; set; }

        public IEnumerable<TimeAbsenceDto> Absences { get; set; }

        public int? AutoFillProjectId { get; set; }

        public IEnumerable<TimeProjectOptionDto> AvailableProjects { get; set; }
    }
}
