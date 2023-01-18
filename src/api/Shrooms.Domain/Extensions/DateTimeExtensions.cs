using System;

namespace Shrooms.Domain.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ConvertToTimeZone(this DateTime date, string timeZoneKey)
        {
            if (date.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException($"Expected date kind to be {DateTimeKind.Utc}. Received {date.Kind}.");
            }
            
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneKey);
            return TimeZoneInfo.ConvertTimeFromUtc(date, timeZone);
        }
    }
}
