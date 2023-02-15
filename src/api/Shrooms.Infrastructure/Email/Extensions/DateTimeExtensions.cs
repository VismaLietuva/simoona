using System;

namespace Shrooms.Infrastructure.Email.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ConvertUtcToTimeZone(this DateTime date, string timeZoneKey)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneKey);
            return TimeZoneInfo.ConvertTimeFromUtc(date, timeZone);
        }
    }
}
