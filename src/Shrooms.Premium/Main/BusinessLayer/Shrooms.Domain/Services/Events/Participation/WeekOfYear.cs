using System;
using System.Globalization;

namespace Shrooms.Domain.Services.Events.Participation
{
    public static class WeekOfYear
    {
        public static int GetNumber(DateTime time)
        {
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}
