using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Constants;

namespace Shrooms.Premium.Tests.DataLayer
{
    public class EventWaitlistModelTests
    {
        private ShroomsDbContext _context;

        [SetUp]
        public void TestInitializer()
        {
            var options = new DbContextOptionsBuilder<ShroomsDbContext>()
                .UseInMemoryDatabase(databaseName: "EventWaitlistModelTests")
                .Options;

            _context = new ShroomsDbContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public void Should_Store_HasWaitlist_On_Event_As_Required_Bool()
        {
            var property = _context.Model.FindEntityType(typeof(Event)).FindProperty(nameof(Event.HasWaitlist));

            Assert.That(property, Is.Not.Null);
            Assert.That(property.IsNullable, Is.False);
        }

        [Test]
        public void Should_Store_WaitlistedAt_On_Participant_As_Nullable()
        {
            var property = _context.Model.FindEntityType(typeof(EventParticipant)).FindProperty(nameof(EventParticipant.WaitlistedAt));

            Assert.That(property, Is.Not.Null);
            Assert.That(property.IsNullable, Is.True);
        }

        [Test]
        public void Should_Append_Waitlisted_After_Existing_Statuses()
        {
            Assert.That((int)AttendingStatus.AttendingVirtually, Is.EqualTo(4));
            Assert.That((int)AttendingStatus.Waitlisted, Is.EqualTo(5));
        }
    }
}
