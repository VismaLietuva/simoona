using System;
using NUnit.Framework;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.Tests.Entities
{
    [TestFixture]
    public class EventTests
    {
        [Test]
        public void LocalStartDate_UsesTheResponsibleUsersTimeZone()
        {
            var @event = new Event
            {
                StartDate = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc),
                ResponsibleUser = new ApplicationUser { Settings = new UserSettings { TimeZone = "FLE Standard Time" } }
            };

            Assert.That(@event.LocalStartDate, Is.EqualTo(new DateTime(2026, 7, 1, 12, 0, 0)));
        }

        [Test]
        public void LocalStartDate_WithoutResponsibleUser_UsesTheDefaultTimeZone()
        {
            var @event = new Event { StartDate = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc) };

            Assert.That(@event.LocalStartDate, Is.EqualTo(new DateTime(2026, 7, 1, 12, 0, 0)));
        }

        [Test]
        public void LocalStartDate_ResponsibleUserSettingsNotLoaded_Throws()
        {
            var @event = new Event
            {
                StartDate = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc),
                ResponsibleUser = new ApplicationUser()
            };

            Assert.Throws<InvalidOperationException>(() => _ = @event.LocalStartDate);
        }
    }
}
