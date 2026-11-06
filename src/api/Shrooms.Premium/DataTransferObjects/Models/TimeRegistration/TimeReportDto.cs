using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.TimeRegistration
{
    public class TimeReportDto
    {
        public string Month { get; set; }

        public IList<TimeReportEmployeeDto> Employees { get; set; }

        public IList<TimeReportProjectDto> Projects { get; set; }
    }
}
