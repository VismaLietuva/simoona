using System;
using TimeZoneConverter;

namespace Shrooms.Infrastructure.Email.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ConvertUtcToTimeZone(this DateTime date, string timeZoneKey)
        {
            if (string.IsNullOrEmpty(timeZoneKey))
            {
                return date;
            }

            var timeZone = TZConvert.GetTimeZoneInfo(timeZoneKey);
            return TimeZoneInfo.ConvertTimeFromUtc(date, timeZone);
        }
    }
}
