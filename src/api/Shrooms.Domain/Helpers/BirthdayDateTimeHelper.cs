using System;

namespace Shrooms.Domain.Helpers
{
    public static class BirthdayDateTimeHelper
    {
        public static DateTime? RemoveYear(DateTime? date)
        {
            if (date.HasValue)
            {
                return new DateTime(1904, date.Value.Month, date.Value.Day);
            }

            return null;
        }
    }
}
