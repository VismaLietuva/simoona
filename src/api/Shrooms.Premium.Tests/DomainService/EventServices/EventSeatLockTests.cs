using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Domain.Services.Events.Participation;

namespace Shrooms.Premium.Tests.DomainService.EventServices
{
    // The in-memory provider is not relational, so these cover the lock's scoping and after-commit
    // behaviour; the sp_getapplock path itself needs SQL Server.
    [TestFixture]
    public class EventSeatLockTests
    {
        private ShroomsDbContext _context;
        private EventSeatLock _sut;

        [SetUp]
        public void TestInitializer()
        {
            var options = new DbContextOptionsBuilder<ShroomsDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(EventSeatLockTests) + Guid.NewGuid())
                .Options;
            _context = new ShroomsDbContext(options);

            var uow = Substitute.For<IUnitOfWork2>();
            uow.GetDbSet<Event>().Returns(_context.Set<Event>());

            _sut = new EventSeatLock(uow, Substitute.For<ILogger<EventSeatLock>>());
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Should_Run_The_Work()
        {
            var ran = false;

            await _sut.RunAsync(Guid.NewGuid(), () =>
            {
                ran = true;
                return Task.CompletedTask;
            });

            Assert.That(ran, Is.True);
        }

        [Test]
        public void Should_Run_An_After_Commit_Action_At_Once_When_No_Lock_Is_Held()
        {
            var ran = false;

            _sut.AfterCommit(() => ran = true);

            Assert.That(ran, Is.True);
        }

        [Test]
        public async Task Should_Run_An_After_Commit_Action_Only_After_The_Work_Has_Finished()
        {
            var order = new List<string>();

            await _sut.RunAsync(Guid.NewGuid(), () =>
            {
                _sut.AfterCommit(() => order.Add("after"));
                order.Add("work");
                return Task.CompletedTask;
            });

            Assert.That(order, Is.EqualTo(new[] { "work", "after" }));
        }

        [Test]
        public void Should_Drop_An_After_Commit_Action_When_The_Work_Fails()
        {
            var ran = false;

            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.RunAsync(Guid.NewGuid(), () =>
            {
                _sut.AfterCommit(() => ran = true);
                throw new InvalidOperationException();
            }));

            Assert.That(ran, Is.False);
        }

        [Test]
        public async Task Should_Join_The_Outer_Lock_When_The_Same_Event_Asks_Again_And_Wait_For_The_Outermost_Commit()
        {
            var eventId = Guid.NewGuid();
            var order = new List<string>();

            await _sut.RunAsync(eventId, async () =>
            {
                await _sut.RunAsync(eventId, () =>
                {
                    _sut.AfterCommit(() => order.Add("after"));
                    order.Add("inner");
                    return Task.CompletedTask;
                });

                order.Add("outer");
            });

            Assert.That(order, Is.EqualTo(new[] { "inner", "outer", "after" }));
        }

        [Test]
        public void Should_Refuse_To_Take_A_Second_Events_Lock_While_Holding_One()
        {
            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.RunAsync(Guid.NewGuid(), () => _sut.RunAsync(Guid.NewGuid(), () => Task.CompletedTask)));
        }

        [Test]
        public async Task Should_Release_The_Lock_After_A_Failure()
        {
            Assert.ThrowsAsync<InvalidOperationException>(() => _sut.RunAsync(Guid.NewGuid(), () => throw new InvalidOperationException()));

            var ran = false;
            await _sut.RunAsync(Guid.NewGuid(), () =>
            {
                ran = true;
                return Task.CompletedTask;
            });

            Assert.That(ran, Is.True);
        }
    }
}
