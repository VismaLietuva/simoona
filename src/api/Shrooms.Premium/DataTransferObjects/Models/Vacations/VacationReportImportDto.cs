using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Vacations
{
    /// <summary>
    /// The report of one payroll-report import. Transient — the UI shows it once
    /// and forgets it; only the requests it created are persisted.
    /// </summary>
    public class VacationReportImportDto
    {
        public string FileName { get; set; }

        public IList<VacationReportRowDto> Imported { get; set; }

        public IList<VacationReportRowDto> Duplicates { get; set; }

        public IList<VacationReportRowErrorDto> Errors { get; set; }
    }

    public class VacationReportRowDto
    {
        public int Line { get; set; }

        public string Name { get; set; }

        public string EmployeeId { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public string Type { get; set; }

        public double WorkingDays { get; set; }
    }

    public class VacationReportRowErrorDto
    {
        public int Line { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// A code the client renders its own sentence for: unknownEmployee,
        /// badDates, badType, noWorkingDays or overlap.
        /// </summary>
        public string Reason { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }
    }
}
