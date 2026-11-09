namespace Shrooms.Premium.DataTransferObjects.Models.TimeRegistration
{
    public class TimeOvertimeRequestInputDto
    {
        public string EmployeeId { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public int MinutesPerDay { get; set; }

        public string Reason { get; set; }
    }
}
