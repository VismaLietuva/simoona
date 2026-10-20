using System.Globalization;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public static class TimeRegistrationRules
    {
        public const int FullDayMinutes = 8 * 60;
        public const int MaxDayMinutes = 24 * 60;
        public const int ReminderWorkingDays = 2;

        public static bool SameName(string a, string b)
        {
            return string.Compare(a?.Trim(), b?.Trim(), CultureInfo.InvariantCulture, CompareOptions.IgnoreCase) == 0;
        }

        public static bool SameUser(string a, string b)
        {
            return string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
        }

        public static int DailyMinutes(bool hasWorkingHours, bool fullTime, int? partTimeHours)
        {
            return hasWorkingHours && !fullTime && partTimeHours > 0
                ? partTimeHours.Value * 60
                : FullDayMinutes;
        }
    }
}
