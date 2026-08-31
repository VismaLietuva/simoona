using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Vacations
{
    /// <summary>
    /// One employee's annual-leave position. All figures are working days.
    ///
    /// <see cref="Taken"/> + <see cref="Upcoming"/> deliberately does not equal
    /// <see cref="Booked"/>: booked also carries pending requests and is measured
    /// from the payslip cutoff, whereas these two describe the whole approved
    /// history either side of today. Leave in progress right now falls in
    /// neither.
    /// </summary>
    public class VacationStatsDto
    {
        public VacationPersonDto Employee { get; set; }

        public double Accrued { get; set; }

        public double Booked { get; set; }

        public double Remaining { get; set; }

        public double Taken { get; set; }

        public double Upcoming { get; set; }

        public int PendingCount { get; set; }

        public int YearsOfService { get; set; }
    }

    public class VacationStatisticsDto
    {
        public IList<VacationStatsDto> Rows { get; set; }

        public VacationStatsTotalsDto Totals { get; set; }

        /// <summary>
        /// The most recent payslip date among the employees in scope, or null
        /// when none of them has ever been imported.
        ///
        /// Every row is measured at its *own* employee's payslip, so this is the
        /// newest of those rather than one date they all share — which is why
        /// the screen words it as "latest" rather than stating a single cutoff.
        /// </summary>
        public string BalanceAsOf { get; set; }
    }

    public class VacationStatsTotalsDto
    {
        public double Accrued { get; set; }

        public double Booked { get; set; }

        public double Remaining { get; set; }

        public double Taken { get; set; }

        public double Upcoming { get; set; }

        public int PendingCount { get; set; }
    }
}
