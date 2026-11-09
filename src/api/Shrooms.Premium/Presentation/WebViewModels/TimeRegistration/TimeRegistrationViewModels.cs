using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;

namespace Shrooms.Premium.Presentation.WebViewModels.TimeRegistration
{
    public class TimeMonthViewModel
    {
        public const string MonthPattern = @"^\d{4}-(0[1-9]|1[0-2])$";

        [Required]
        [RegularExpression(MonthPattern)]
        public string Month { get; set; }
    }

    public class TimeEmployeeMonthViewModel : TimeMonthViewModel
    {
        [Required]
        public string EmployeeId { get; set; }
    }

    public class TimeEntriesViewModel : TimeMonthViewModel
    {
        public string EmployeeId { get; set; }

        [Required]
        public IList<TimeEntryChangeDto> Entries { get; set; }
    }

    public class TimeProjectChoiceViewModel
    {
        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        public string EmployeeId { get; set; }
    }

    public class TimeOvertimeRequestViewModel
    {
        public const string DayPattern = @"^\d{4}-\d{2}-\d{2}$";

        public string EmployeeId { get; set; }

        [Required]
        [RegularExpression(DayPattern)]
        public string DateFrom { get; set; }

        [Required]
        [RegularExpression(DayPattern)]
        public string DateTo { get; set; }

        [Range(1, 24 * 60)]
        public int MinutesPerDay { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }
    }

    public class TimeOvertimeRejectViewModel
    {
        [Required]
        [MaxLength(500)]
        public string Comment { get; set; }
    }

    public class TimeArchiveViewModel
    {
        [Required]
        public bool? IsArchived { get; set; }
    }
}
