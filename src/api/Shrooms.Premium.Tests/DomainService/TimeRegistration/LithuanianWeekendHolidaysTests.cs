using System;
using System.Linq;
using NUnit.Framework;
using Shrooms.Premium.Domain.Services.TimeRegistration;

namespace Shrooms.Premium.Tests.DomainService.TimeRegistration
{
    [TestFixture]
    public class LithuanianWeekendHolidaysTests
    {
        [Test]
        public void For_ReturnsOnlyTheHolidaysThatFallOnAWeekend()
        {
            var holidays = LithuanianWeekendHolidays.For(2026);

            Assert.That(holidays.Keys.OrderBy(day => day), Is.EqualTo(new[]
            {
                new DateTime(2026, 4, 5),
                new DateTime(2026, 5, 3),
                new DateTime(2026, 6, 7),
                new DateTime(2026, 8, 15),
                new DateTime(2026, 11, 1),
                new DateTime(2026, 12, 26)
            }));
            Assert.That(holidays[new DateTime(2026, 4, 5)], Is.EqualTo("Šv. Velykos"));
        }
    }
}
