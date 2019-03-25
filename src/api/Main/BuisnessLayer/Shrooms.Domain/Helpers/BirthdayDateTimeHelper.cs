using System;

namespace Shrooms.Domain.Helpers
{
    public static class BirthdayDateTimeHelper
    {
        public static DateTime? RemoveYear(DateTime? date)
        {
            if (date != null)
            {
                return new DateTime(1904, date.Value.Month, date.Value.Day);
            }

            return date;
        }
    }
}
