using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Group;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Premium.Domain.Services.Groups;
using Shrooms.Tests.Extensions;
using GroupEntity = Shrooms.DataLayer.EntityModels.Models.Group.Group;

namespace Shrooms.Premium.Tests.DomainService
{
    public class GroupKudosServiceTests
    {
        private const int Year = 2026;
        private const int Month = 8;

        private IGroupKudosService _service;
        private IUnitOfWork2 _uow;
        private DbSet<GroupEntity> _groupsDbSet;
        private DbSet<KudosLog> _kudosLogsDbSet;
        private DbSet<KudosType> _kudosTypesDbSet;

        // The kudos type now lives on the group type, and is worth 5 here.
        private static readonly KudosType Monthly = new KudosType { Id = 3, Name = "Monthly", Value = 5 };

        private static GroupType KudosReceivingType() => new GroupType
        {
            Id = 3,
            OrganizationId = 1,
            Name = "FoodMaster",
            KudosTypeId = Monthly.Id,
            KudosType = Monthly
        };

        private static GroupMember Member(string userId, DateTime? start = null, DateTime? end = null) =>
            new GroupMember { UserId = userId, StartDate = start, EndDate = end };

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();

            _groupsDbSet = Substitute.For<DbSet<GroupEntity>, IQueryable<GroupEntity>, IAsyncEnumerable<GroupEntity>>();
            _groupsDbSet.SetDbSetDataForAsync(MockGroups());
            _uow.GetDbSet<GroupEntity>().Returns(_groupsDbSet);

            _kudosLogsDbSet = Substitute.For<DbSet<KudosLog>, IQueryable<KudosLog>, IAsyncEnumerable<KudosLog>>();
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog>());
            _uow.GetDbSet<KudosLog>().Returns(_kudosLogsDbSet);

            _kudosTypesDbSet = Substitute.For<DbSet<KudosType>, IQueryable<KudosType>, IAsyncEnumerable<KudosType>>();
            _kudosTypesDbSet.SetDbSetDataForAsync(new List<KudosType> { Monthly });
            _uow.GetDbSet<KudosType>().Returns(_kudosTypesDbSet);

            _service = new GroupKudosService(_uow);
        }

        private static GroupEntity KudosGroup(int id, string name, params GroupMember[] members) => new GroupEntity
        {
            Id = id,
            OrganizationId = 1,
            Name = name,
            GroupTypeId = 3,
            GroupType = KudosReceivingType(),
            Status = GroupStatus.Approved,
            Members = members.ToList()
        };

        // Alice is in all three food teams, Bob in one. Neither is paid for the book club.
        private static IList<GroupEntity> MockGroups() => new List<GroupEntity>
        {
            KudosGroup(1, "Team A", Member("alice"), Member("bob")),
            KudosGroup(2, "Team B", Member("alice")),
            KudosGroup(3, "Team C", Member("alice")),
            new GroupEntity
            {
                Id = 4, OrganizationId = 1, Name = "Book club", GroupTypeId = 1,
                GroupType = new GroupType { Id = 1, OrganizationId = 1, Name = "Other" },
                Status = GroupStatus.Approved,
                Members = new List<GroupMember> { Member("alice"), Member("bob") }
            }
        };

        [Test]
        public async Task Should_Sum_Kudos_Type_Value_Across_A_Users_Groups()
        {
            var allocations = (await _service.GetAllocationsAsync(1, Year, Month)).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(allocations.Single(a => a.UserId == "alice").Amount, Is.EqualTo(15));
                Assert.That(allocations.Single(a => a.UserId == "bob").Amount, Is.EqualTo(5));
            });
        }

        [Test]
        public async Task Should_Give_Each_User_One_Allocation_Per_Kudos_Type()
        {
            var allocations = (await _service.GetAllocationsAsync(1, Year, Month)).ToList();

            Assert.That(allocations.Select(a => a.UserId), Is.Unique);
            Assert.That(allocations, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task Should_Exclude_Temporary_Groups_From_The_Monthly_Run()
        {
            // A task force is paid once at the end of its term, not every month.
            var temporaryType = KudosReceivingType();
            temporaryType.IsTemporary = true;

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                new GroupEntity
                {
                    Id = 5, OrganizationId = 1, Name = "Task force", GroupTypeId = 3,
                    GroupType = temporaryType,
                    Status = GroupStatus.Approved,
                    Members = new List<GroupMember> { Member("alice") }
                }
            });

            var allocations = (await _service.GetAllocationsAsync(1, Year, Month)).ToList();

            Assert.That(allocations, Is.Empty);
        }

        [Test]
        public async Task Should_Exclude_Groups_Still_Awaiting_Approval()
        {
            // A proposal has not earned anything yet, so it pays nobody until it is approved.
            var pending = KudosGroup(6, "Proposed team", Member("alice"));
            pending.Status = GroupStatus.Pending;

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                pending,
                KudosGroup(1, "Team A", Member("bob"))
            });

            var allocations = (await _service.GetAllocationsAsync(1, Year, Month)).ToList();

            Assert.That(allocations.Select(a => a.UserId), Is.EquivalentTo(new[] { "bob" }));
        }

        [Test]
        public async Task Should_Exclude_Group_Types_Without_A_Kudos_Type()
        {
            var allocations = (await _service.GetAllocationsAsync(1, Year, Month)).ToList();

            Assert.That(allocations.SelectMany(a => a.GroupNames), Does.Not.Contain("Book club"));
        }

        [Test]
        public async Task Should_Count_A_Group_Once_When_A_User_Has_Several_Memberships_Of_It()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                KudosGroup(
                    1,
                    "Team A",
                    Member("alice", end: new DateTime(2026, 8, 10)),
                    Member("alice", start: new DateTime(2026, 8, 20)))
            });

            var allocations = (await _service.GetAllocationsAsync(1, Year, Month)).ToList();

            Assert.That(allocations.Single().Amount, Is.EqualTo(5));
        }

        [Test]
        public async Task Should_Exclude_Members_Whose_Membership_Ended_Before_The_Period()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                KudosGroup(1, "Team A", Member("alice", end: new DateTime(2026, 6, 30)), Member("bob"))
            });

            var allocations = (await _service.GetAllocationsAsync(1, Year, Month)).ToList();

            Assert.That(allocations.Select(a => a.UserId), Is.EquivalentTo(new[] { "bob" }));
        }

        [Test]
        public async Task Should_Exclude_Members_Who_Join_After_The_Period()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                KudosGroup(1, "Team A", Member("alice", start: new DateTime(2026, 9, 1)), Member("bob"))
            });

            var allocations = (await _service.GetAllocationsAsync(1, Year, Month)).ToList();

            Assert.That(allocations.Select(a => a.UserId), Is.EquivalentTo(new[] { "bob" }));
        }

        [Test]
        public async Task Should_Include_Member_Who_Left_Partway_Through_The_Period()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                KudosGroup(1, "Team A", Member("alice", end: new DateTime(2026, 8, 15)))
            });

            var allocations = (await _service.GetAllocationsAsync(1, Year, Month)).ToList();

            Assert.That(allocations.Select(a => a.UserId), Is.EquivalentTo(new[] { "alice" }));
        }

        [Test]
        public async Task Should_Write_One_Pending_Kudos_Log_Per_Allocated_Member()
        {
            var userAndOrg = new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" };

            var result = await _service.AwardMonthlyKudosAsync(userAndOrg, Year, Month);

            Assert.Multiple(() =>
            {
                Assert.That(result.AwardedCount, Is.EqualTo(2));
                Assert.That(result.TotalAmount, Is.EqualTo(20));
            });

            // Pending, so a kudos administrator still approves the monthly run. Approval is
            // what recomputes the profile balance; nothing here should touch it.
            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(l =>
                l.EmployeeId == "alice" && l.Points == 15 && l.Status == KudosStatus.Pending));
            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(l =>
                l.EmployeeId == "bob" && l.Points == 5 && l.Status == KudosStatus.Pending));
        }

        [Test]
        public async Task Should_Not_Save_When_Nothing_Was_Awarded()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>());

            var result = await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            Assert.That(result.AwardedCount, Is.Zero);

            await _uow.DidNotReceiveWithAnyArgs().SaveChangesAsync(default(string));
        }

        [TestCase(2026, 13)]
        [TestCase(2026, 0)]
        [TestCase(0, 8)]
        [TestCase(10000, 8)]
        public void Should_Reject_A_Period_That_Is_Not_A_Real_Month(int year, int month)
        {
            // year and month come off the query string; an out-of-range pair used to reach
            // new DateTime(...) and surface as a 500.
            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _service.GetAllocationsAsync(1, year, month));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.GroupInvalidKudosPeriod));
        }
    }
}
