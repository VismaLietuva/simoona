namespace Shrooms.Premium.DataTransferObjects.Models.Vacations
{
    /// <summary>What an employee may set: no status, and every date rule applies.</summary>
    public class VacationRequestDraftDto
    {
        public string Type { get; set; }

        public string DateFrom { get; set; }

        /// <summary>Ignored for single-day types.</summary>
        public string DateTo { get; set; }

        public string Note { get; set; }
    }

    /// <summary>
    /// What an administrator may set. Deliberately wider than the employee
    /// draft: it carries a status, and none of the date rules apply — an
    /// administrator is correcting a record, not asking for leave.
    /// </summary>
    public class VacationAdminPatchDto
    {
        public string Type { get; set; }

        public string Status { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public string Note { get; set; }
    }
}
