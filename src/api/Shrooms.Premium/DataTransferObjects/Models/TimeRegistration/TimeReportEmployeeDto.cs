namespace Shrooms.Premium.DataTransferObjects.Models.TimeRegistration
{
    public class TimeReportEmployeeDto : TimeEmployeeDto
    {
        public string ManagerName { get; set; }

        public string Status { get; set; }

        public int LoggedMinutes { get; set; }

        public int? ExpectedMinutes { get; set; }

        public int? AbsenceMinutes { get; set; }

        public int OvertimeMinutes { get; set; }

        public bool IsFormer { get; set; }
    }
}
