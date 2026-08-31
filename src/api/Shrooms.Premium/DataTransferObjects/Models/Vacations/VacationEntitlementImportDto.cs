using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Vacations
{
    /// <summary>
    /// The report of one entitlement import. Transient — the UI shows it once
    /// and forgets it; nothing about it is persisted.
    /// </summary>
    public class VacationEntitlementImportDto
    {
        public string AsOf { get; set; }

        public string FileName { get; set; }

        public IList<VacationEntitlementEntryDto> Imported { get; set; }

        public IList<VacationEntitlementSkipDto> Skipped { get; set; }

        /// <summary>Lines that could not be parsed at all.</summary>
        public int Unreadable { get; set; }
    }

    public class VacationEntitlementEntryDto
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string EmployeeId { get; set; }

        public double From { get; set; }

        public double To { get; set; }
    }

    public class VacationEntitlementSkipDto
    {
        public string Code { get; set; }

        public string Name { get; set; }
    }
}
