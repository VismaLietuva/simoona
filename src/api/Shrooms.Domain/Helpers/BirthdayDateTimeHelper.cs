using System;

namespace Shrooms.Domain.Helpers
{
    public static class BirthdayDateTimeHelper
    {
        // Leap year, so February 29 birthdays stay valid.
        public const int HiddenYear = 1904;

        public static DateTime? RemoveYear(DateTime? date)
        {
            if (date.HasValue)
            {
                return new DateTime(HiddenYear, date.Value.Month, date.Value.Day);
            }

            return null;
        }
    }
}
