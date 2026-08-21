namespace Shrooms.Premium.DataTransferObjects.Models.Vacations
{
    public class VacationBalanceDto
    {
        public double Entitlement { get; set; }

        public string BalanceAsOf { get; set; }

        public double Booked { get; set; }

        public double Remaining { get; set; }

        /// <summary>
        /// Estimate only (entitlement plus accrual since the cutoff). Never used
        /// for validation — see VacationCalculator.ApproxAccruedNow.
        /// </summary>
        public double AccruedNow { get; set; }

        public double MonthlyAccrualRate { get; set; }

        public int YearsOfService { get; set; }
    }
}
