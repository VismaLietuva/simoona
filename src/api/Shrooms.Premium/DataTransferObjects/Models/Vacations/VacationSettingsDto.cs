namespace Shrooms.Premium.DataTransferObjects.Models.Vacations
{
    /// <summary>Organisation-wide leave-order settings. Accrual is statutory, so it is not settable.</summary>
    public class VacationSettingsDto
    {
        public string OrderPrefix { get; set; }

        /// <summary>
        /// Where the order sequence starts, so numbering can continue from a
        /// paper trail that began outside the application.
        /// </summary>
        public int OrderStartNumber { get; set; }

        public int NextOrderNumber { get; set; }

        public string OrderLetterhead { get; set; }

        public string OrderCity { get; set; }

        public string OrderSignature { get; set; }
    }

    public static class VacationSettingLimits
    {
        public const int MinOrderStartNumber = 1;
        public const int MaxOrderStartNumber = 999999;
    }
}
