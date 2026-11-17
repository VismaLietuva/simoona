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
        public void LocalStartDate_ResponsibleUserWithoutTimeZone_UsesTheDefaultTimeZone()
        {
            var @event = new Event
            {
                StartDate = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc),
                ResponsibleUser = new ApplicationUser { Settings = new UserSettings() }
            };

            Assert.That(@event.LocalStartDate, Is.EqualTo(new DateTime(2026, 7, 1, 12, 0, 0)));
        }

        [Test]
        public void LocalStartDate_ResponsibleUserWithoutSettingsRow_UsesTheDefaultTimeZone()
        {
            var @event = new Event
            {
                StartDate = new DateTime(2026, 7, 1, 9, 0, 0, DateTimeKind.Utc),
                ResponsibleUser = new ApplicationUser()
            };

            Assert.That(@event.LocalStartDate, Is.EqualTo(new DateTime(2026, 7, 1, 12, 0, 0)));
        }

        [Test]
        public void LocalStartDate_Set_WithoutSettingsRow_ConvertsFromTheDefaultTimeZone()
        {
            var @event = new Event
            {
                ResponsibleUser = new ApplicationUser(),
                LocalStartDate = new DateTime(2026, 7, 1, 12, 0, 0)
            };

            Assert.That(@event.StartDate, Is.EqualTo(new DateTime(2026, 7, 1, 9, 0, 0)));
        }
    }
}
