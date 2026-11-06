using System;

namespace Shrooms.Premium.DataTransferObjects.Models.TimeRegistration
{
    public class TimeOvertimeRequestDto
    {
        public int Id { get; set; }

        public string EmployeeId { get; set; }

        public string EmployeeName { get; set; }

        public string EmployeePictureId { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public int MinutesPerDay { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; }

        public bool OrderedByManager { get; set; }

        public string CreatedById { get; set; }

        public string CreatedByName { get; set; }

        public DateTime CreatedAt { get; set; }

        public string DecidedByName { get; set; }

        public DateTime? DecidedAt { get; set; }

        public string ReviewComment { get; set; }
    }
}
