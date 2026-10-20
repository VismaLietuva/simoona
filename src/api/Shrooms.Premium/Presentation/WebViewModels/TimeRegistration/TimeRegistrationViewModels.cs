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

    public class TimeArchiveViewModel
    {
        [Required]
        public bool? IsArchived { get; set; }
    }
}
