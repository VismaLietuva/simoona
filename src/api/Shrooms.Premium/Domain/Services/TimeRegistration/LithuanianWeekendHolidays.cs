using System;
using System.Collections.Generic;
using System.Linq;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    /// <summary>
    /// The holiday table only stores holidays that fall on working days, but the
    /// time table still marks the ones that land on a weekend.
    /// </summary>
    public static class LithuanianWeekendHolidays
    {
        private static readonly (int Month, int Day, string Name)[] Fixed =
        {
            (1, 1, "Naujųjų metų diena"),
            (2, 16, "Lietuvos valstybės atkūrimo diena"),
            (3, 11, "Lietuvos Nepriklausomybės atkūrimo diena"),
            (5, 1, "Tarptautinė darbo diena"),
            (6, 24, "Rasos ir Joninių diena"),
            (7, 6, "Mindaugo karūnavimo diena"),
            (8, 15, "Žolinė"),
            (11, 1, "Visų šventųjų diena"),
            (11, 2, "Mirusiųjų atminimo (Vėlinių) diena"),
            (12, 24, "Kūčių diena"),
            (12, 25, "Šventosios Kalėdos"),
            (12, 26, "Antroji Kalėdų diena")
        };

        public static IReadOnlyDictionary<DateTime, string> For(int year)
        {
            var movable = new (DateTime Date, string Name)[]
            {
                (EasterSunday(year), "Šv. Velykos"),
                (FirstSunday(year, 5), "Motinos diena"),
                (FirstSunday(year, 6), "Tėvo diena")
            };

            return Fixed
                .Select(holiday => (Date: new DateTime(year, holiday.Month, holiday.Day), holiday.Name))
                .Concat(movable)
                .Where(holiday => TimeMonth.IsWeekend(holiday.Date))
                .GroupBy(holiday => holiday.Date)
                .ToDictionary(group => group.Key, group => group.First().Name);
        }

        private static DateTime EasterSunday(int year)
        {
            var a = year % 19;
            var b = year / 100;
            var c = year % 100;
            var d = b / 4;
            var e = b % 4;
            var f = (b + 8) / 25;
            var g = (b - f + 1) / 3;
            var h = ((19 * a) + b - d - g + 15) % 30;
            var i = c / 4;
            var k = c % 4;
            var l = (32 + (2 * e) + (2 * i) - h - k) % 7;
            var m = (a + (11 * h) + (22 * l)) / 451;
            var month = (h + l - (7 * m) + 114) / 31;
            var day = ((h + l - (7 * m) + 114) % 31) + 1;
            return new DateTime(year, month, day);
        }

        private static DateTime FirstSunday(int year, int month)
        {
            var first = new DateTime(year, month, 1);
            return first.AddDays(((int)DayOfWeek.Sunday - (int)first.DayOfWeek + 7) % 7);
        }
    }
}
