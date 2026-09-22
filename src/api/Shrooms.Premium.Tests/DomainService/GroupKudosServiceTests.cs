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

        private static GroupType KudosReceivingType(string awardTemplate = null) => new GroupType
        {
            Id = 3,
            OrganizationId = 1,
            Name = "FoodMaster",
            KudosTypeId = Monthly.Id,
            KudosType = Monthly,
            AwardTemplate = awardTemplate
        };

        // A log the run left behind, stamped with the period it paid for. The stamp is what
        // makes a month count as awarded.
        private static KudosLog AwardedLog(int year, int month, string userId = "alice", decimal points = 5) => new KudosLog
        {
            OrganizationId = 1,
            EmployeeId = userId,
            KudosTypeName = "Monthly",
            Points = points,
            Status = KudosStatus.Pending,
            GroupKudosPeriod = new DateTime(year, month, 1),
            Created = DateTime.UtcNow,
            Comments = "Foodmaster of the month 🍕"
        };

        private static GroupMember Member(string userId, DateTime? start = null, DateTime? end = null, string role = null) =>
            new GroupMember { UserId = userId, StartDate = start, EndDate = end, Description = role };

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

        private static GroupEntity KudosGroup(int id, string name, params GroupMember[] members) =>
            TemplatedKudosGroup(id, name, null, members);

        private static GroupEntity TemplatedKudosGroup(int id, string name, string awardTemplate, params GroupMember[] members) => new GroupEntity
        {
            Id = id,
            OrganizationId = 1,
            Name = name,
            GroupTypeId = 3,
            GroupType = KudosReceivingType(awardTemplate),
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
        public async Task Should_Word_The_Comment_With_The_Group_Types_Template()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                TemplatedKudosGroup(1, "Academic Committee", "For your valuable contribution to the {GroupName} in {Month}! 🏅🙌👏", Member("alice"))
            });

            await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(l =>
                l.Comments == "For your valuable contribution to the Academic Committee in 2026-08! 🏅🙌👏"));
        }

        [Test]
        public async Task Should_Join_Several_Roles_With_A_Pipe()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                TemplatedKudosGroup(1, "Team A", "Foodmaster {month} {role} 🍕", Member("alice", role: "Role1")),
                TemplatedKudosGroup(2, "Team B", "Foodmaster {month} {role} 🍕", Member("alice", role: "Role2")),
                TemplatedKudosGroup(3, "Team C", "Foodmaster {month} {role} 🍕", Member("alice", role: "Role3"))
            });

            await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(l =>
                l.Comments == "Foodmaster 2026-08 Role1 | Role2 | Role3 🍕"));
        }

        [Test]
        public async Task Should_Not_Leave_A_Gap_Where_A_Placeholder_Rendered_Nothing()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                TemplatedKudosGroup(1, "Team A", "Foodmaster {month} {role} 🍕", Member("alice"))
            });

            await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(l => l.Comments == "Foodmaster 2026-08 🍕"));
        }

        [Test]
        public async Task Should_Fall_Back_To_Plain_Wording_When_The_Type_Has_No_Template()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                KudosGroup(1, "Team A", Member("alice"))
            });

            await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(l =>
                l.Comments == "Monthly group kudos for 2026-08: Team A"));
        }

        [Test]
        public async Task Should_Write_A_Log_Per_Group_Type_Rather_Than_Per_Kudos_Type()
        {
            var committee = TemplatedKudosGroup(1, "Academic Committee", "Committee: {groupname}", Member("alice"));
            committee.GroupTypeId = 4;
            committee.GroupType.Id = 4;

            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                TemplatedKudosGroup(2, "Team A", "Foodmaster {month}", Member("alice")),
                committee
            });

            var result = await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            Assert.That(result.AwardedCount, Is.EqualTo(2));

            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(l => l.Comments == "Foodmaster 2026-08"));
            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(l => l.Comments == "Committee: Academic Committee"));
        }

        [Test]
        public async Task Should_Not_Save_When_Nothing_Was_Awarded()
        {
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>());

            var result = await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            Assert.That(result.AwardedCount, Is.Zero);

            _kudosLogsDbSet.DidNotReceiveWithAnyArgs().Add(default);
            await _uow.DidNotReceiveWithAnyArgs().SaveChangesAsync(default(string));
        }

        [Test]
        public async Task Should_Stamp_Every_Log_With_The_Period_It_Pays_For()
        {
            await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            _kudosLogsDbSet.Received(2).Add(Arg.Is<KudosLog>(l =>
                l.GroupKudosPeriod == new DateTime(Year, Month, 1)));
        }

        [Test]
        public async Task Should_Not_Award_A_Period_Twice()
        {
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog>
            {
                AwardedLog(Year, Month, "alice", 15),
                AwardedLog(Year, Month, "bob")
            });

            var result = await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            Assert.Multiple(() =>
            {
                Assert.That(result.AlreadyAwarded, Is.True);
                Assert.That(result.AwardedCount, Is.Zero);
            });

            _kudosLogsDbSet.DidNotReceiveWithAnyArgs().Add(default);
            await _uow.DidNotReceiveWithAnyArgs().SaveChangesAsync(default(string));
        }

        [Test]
        public async Task Should_Award_A_Period_Again_When_Every_Log_For_It_Was_Rejected()
        {
            // Rejecting the batch is how a kudos admin says the run was wrong. The stamp
            // would otherwise keep the month awarded with nobody paid for it.
            var alice = AwardedLog(Year, Month, "alice", 15);
            var bob = AwardedLog(Year, Month, "bob");

            alice.Status = KudosStatus.Rejected;
            bob.Status = KudosStatus.Rejected;

            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog> { alice, bob });

            var result = await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            Assert.Multiple(() =>
            {
                Assert.That(result.AlreadyAwarded, Is.False);
                Assert.That(result.AwardedCount, Is.EqualTo(2));
            });
        }

        [Test]
        public async Task Should_Keep_A_Period_Awarded_When_Only_Some_Of_Its_Logs_Were_Rejected()
        {
            // Re-running would pay everyone whose log survived a second time.
            var alice = AwardedLog(Year, Month, "alice", 15);

            alice.Status = KudosStatus.Rejected;

            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog> { alice, AwardedLog(Year, Month, "bob") });

            var result = await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            Assert.That(result.AlreadyAwarded, Is.True);

            _kudosLogsDbSet.DidNotReceiveWithAnyArgs().Add(default);
        }

        [Test]
        public async Task Should_Treat_A_Fully_Rejected_Month_As_Outstanding_In_A_Catch_Up()
        {
            var rejected = AwardedLog(2026, 8);

            rejected.Status = KudosStatus.Rejected;

            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog> { rejected, AwardedLog(2026, 7) });

            var result = await _service.AwardOutstandingMonthsAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, 2026, 9);

            Assert.That(
                result.Select(p => (p.Year, p.Month)),
                Is.EqualTo(new[] { (2026, 8), (2026, 9) }));
        }

        [Test]
        public async Task Should_Not_Let_Kudos_Of_The_Same_Type_Mark_A_Period_Awarded()
        {
            // An admin handing out "Monthly" kudos by hand used to look exactly like the
            // monthly run, which silently skipped the month for the whole organization.
            // An unstamped log now says nothing about any period.
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog>
            {
                new KudosLog
                {
                    OrganizationId = 1,
                    EmployeeId = "alice",
                    KudosTypeName = "Monthly",
                    Points = 5,
                    Status = KudosStatus.Pending,
                    Created = new DateTime(2026, 10, 3),
                    Comments = "Thanks!"
                }
            });

            var result = await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            Assert.Multiple(() =>
            {
                Assert.That(result.AlreadyAwarded, Is.False);
                Assert.That(result.AwardedCount, Is.EqualTo(2));
            });
        }

        [Test]
        public async Task Should_Backfill_An_Older_Month_After_A_Newer_One_Was_Awarded()
        {
            // September's logs are dated after July's month end, which used to report July
            // as already awarded and pay nobody.
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog> { AwardedLog(2026, 9) });

            var result = await _service.AwardMonthlyKudosAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, 2026, 7);

            Assert.Multiple(() =>
            {
                Assert.That(result.AlreadyAwarded, Is.False);
                Assert.That(result.AwardedCount, Is.EqualTo(2));
            });
        }

        [Test]
        public async Task Should_Award_The_Given_Month_And_Every_Unawarded_Month_Before_It()
        {
            var result = await _service.AwardOutstandingMonthsAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, 2026, 9);

            Assert.That(
                result.Select(p => (p.Year, p.Month)),
                Is.EqualTo(new[] { (2026, 7), (2026, 8), (2026, 9) }));
        }

        [Test]
        public async Task Should_Award_Every_Month_Of_A_Catch_Up_Even_Though_Its_Own_Logs_Are_Dated_Now()
        {
            // The logs written for the older months are dated now, which made the newer ones
            // look awarded. Found against a real database: August was silently skipped.
            var written = new List<KudosLog>();
            _kudosLogsDbSet.When(x => x.Add(Arg.Any<KudosLog>())).Do(call =>
            {
                written.Add(call.Arg<KudosLog>());
                _kudosLogsDbSet.SetDbSetDataForAsync(written.ToList());
            });

            var result = await _service.AwardOutstandingMonthsAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, 2026, 9);

            Assert.That(result.Where(p => p.AlreadyAwarded), Is.Empty);
            Assert.That(
                result.Select(p => (p.Year, p.Month)),
                Is.EqualTo(new[] { (2026, 7), (2026, 8), (2026, 9) }));
        }

        [Test]
        public async Task Should_Skip_Only_The_Months_That_Were_Already_Awarded()
        {
            // Also the shape of a catch-up that died after July: August and September
            // are still outstanding and must be paid on the next run.
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog> { AwardedLog(2026, 7) });

            var result = await _service.AwardOutstandingMonthsAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, 2026, 9);

            Assert.That(result.Where(p => p.AwardedCount == 0), Is.Empty);
            Assert.That(
                result.Select(p => (p.Year, p.Month)),
                Is.EqualTo(new[] { (2026, 8), (2026, 9) }));
        }

        [Test]
        public async Task Should_Fill_A_Gap_Left_By_A_Month_Awarded_On_Its_Own()
        {
            // July and September are done, August is not. Stopping at the first awarded
            // month would leave August unpaid forever.
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog>
            {
                AwardedLog(2026, 7),
                AwardedLog(2026, 9)
            });

            var result = await _service.AwardOutstandingMonthsAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, 2026, 9);

            Assert.That(
                result.Select(p => (p.Year, p.Month)),
                Is.EqualTo(new[] { (2026, 8) }));
        }

        [Test]
        public async Task Should_Not_Walk_Past_The_Earliest_Awardable_Month()
        {
            var result = await _service.AwardOutstandingMonthsAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, 2026, 6);

            Assert.That(result, Is.Empty);

            _kudosLogsDbSet.DidNotReceiveWithAnyArgs().Add(default);
        }

        [Test]
        public async Task Should_Not_Back_Pay_Every_Month_Since_The_Floor()
        {
            // A group type given a kudos type long after the floor would otherwise back-pay
            // every month since it, because an open-ended membership is active in all of them.
            var result = await _service.AwardOutstandingMonthsAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, 2027, 3);

            Assert.That(
                result.Select(p => (p.Year, p.Month)),
                Is.EqualTo(new[] { (2027, 1), (2027, 2), (2027, 3) }));
        }

        [Test]
        public async Task Should_Stop_Offering_A_Month_That_Allocated_Nothing_Once_It_Leaves_The_Window()
        {
            // An empty month leaves no log, so nothing records that it ran. The window is what
            // closes it: three firings later it is out of reach.
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>());

            var withinWindow = await _service.AwardOutstandingMonthsAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, 2026, 9);

            var pastWindow = await _service.AwardOutstandingMonthsAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, 2026, 12);

            Assert.Multiple(() =>
            {
                Assert.That(withinWindow.Select(p => p.Month), Does.Contain(7));
                Assert.That(pastWindow.Select(p => p.Month), Does.Not.Contain(7));
            });
        }

        [Test]
        public async Task Should_Award_Nothing_When_The_Month_That_Just_Ended_Is_Already_Awarded()
        {
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog>
            {
                AwardedLog(Year, Month),
                AwardedLog(2026, 7)
            });

            var result = await _service.AwardOutstandingMonthsAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, Year, Month);

            Assert.That(result, Is.Empty);

            _kudosLogsDbSet.DidNotReceiveWithAnyArgs().Add(default);
        }

        [Test]
        public async Task Should_Ignore_Logs_Of_Another_Organization_When_Walking_Back()
        {
            var otherTenant = AwardedLog(2026, 7);
            otherTenant.OrganizationId = 2;

            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog> { otherTenant });

            var result = await _service.AwardOutstandingMonthsAsync(
                new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" }, 2026, 8);

            Assert.That(
                result.Select(p => (p.Year, p.Month)),
                Is.EqualTo(new[] { (2026, 7), (2026, 8) }));
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
