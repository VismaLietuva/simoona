using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;
using Shrooms.Premium.Domain.Services.TimeRegistration;

namespace Shrooms.Premium.Tests.DomainService.TimeRegistration
{
    [TestFixture]
    public class TimePeriodServiceTests : TimeRegistrationTestBase
    {
        private TimePeriodService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new TimePeriodService(Uow, Support);
        }

        [Test]
        public async Task SubmitAsync_FillsTheRestOfEachWorkingDayOnTheAutoFillProject()
        {
            AddEntry(Ona, ShopId, ReviewId, new DateTime(2026, 10, 1), 90);

            await _service.SubmitAsync(CurrentMonth, AsOna);

            var entries = EntriesOf(Ona);
            Assert.That(entries.Where(entry => entry.TaskId == DevelopmentId).ToList(), Has.Count.EqualTo(22));
            Assert.That(entries.Single(entry => entry.TaskId == DevelopmentId && entry.Date.Day == 1).Minutes, Is.EqualTo(390));
            Assert.That(entries.Sum(entry => entry.Minutes), Is.EqualTo(22 * 480));
            Assert.That(await Support.StatusAsync(Ona, new DateTime(2026, 10, 1), 1), Is.EqualTo(TimePeriodStatus.Submitted));
        }

        [Test]
        public async Task SubmitAsync_SkipsVacationDaysAndUsesPartTimeHours()
        {
            Db.Set<VacationRequest>().Add(new VacationRequest
            {
                OrganizationId = 1,
                EmployeeId = Jonas,
                Type = VacationRequestType.Annual,
                Status = VacationRequestStatus.Approved,
                DateFrom = new DateTime(2026, 10, 5),
                DateTo = new DateTime(2026, 10, 9)
            });
            Db.SaveChanges(false);

            await _service.SubmitAsync(CurrentMonth, AsJonas);

            var entries = EntriesOf(Jonas);
            Assert.That(entries, Has.Count.EqualTo(17));
            Assert.That(entries.All(entry => entry.Minutes == 360), Is.True);
        }

        [Test]
        public void SubmitAsync_RejectsAFutureMonth()
        {
            var error = Assert.ThrowsAsync<ValidationException>(() => _service.SubmitAsync(FutureMonth, AsOna));

            Assert.That(error.ErrorMessage, Is.EqualTo("A future month cannot be submitted"));
        }

        [Test]
        public async Task WithdrawAsync_RejectsAPastMonth()
        {
            await _service.SubmitAsync(PastMonth, AsOna);

            var error = Assert.ThrowsAsync<ValidationException>(() => _service.WithdrawAsync(PastMonth, AsOna));

            Assert.That(error.ErrorMessage, Is.EqualTo("A past month cannot be withdrawn"));
        }

        [Test]
        public void ApproveAsync_RefusesTheApproversOwnMonth()
        {
            var error = Assert.ThrowsAsync<ValidationException>(() => _service.ApproveAsync(Boss, CurrentMonth, AsBoss));

            Assert.That(error.ErrorMessage, Is.EqualTo("You cannot approve your own month"));
        }

        [Test]
        public void ApproveAsync_RefusesSomeoneWhoIsNotYourReport()
        {
            Assert.ThrowsAsync<UnauthorizedException>(() => _service.ApproveAsync(Ona, CurrentMonth, AsJonas));
        }

        [Test]
        public async Task UnapproveAsync_RestoresTheStatusBeforeApproval()
        {
            await _service.SubmitAsync(CurrentMonth, AsOna);
            await _service.ApproveAsync(Ona, CurrentMonth, AsBoss);

            await _service.UnapproveAsync(Ona, CurrentMonth, AsBoss);

            Assert.That(await Support.StatusAsync(Ona, new DateTime(2026, 10, 1), 1), Is.EqualTo(TimePeriodStatus.Submitted));
        }

        [Test]
        public async Task UnapproveAsync_LeavesAPastMonthToAdministrators()
        {
            await _service.ApproveAsync(Ona, PastMonth, AsBoss);

            var error = Assert.ThrowsAsync<ValidationException>(() => _service.UnapproveAsync(Ona, PastMonth, AsBoss));
            Assert.That(error.ErrorMessage, Is.EqualTo("Only an administrator can unapprove a past month"));

            MakeAccountant(AsBoss);
            await _service.UnapproveAsync(Ona, PastMonth, AsBoss);
            Assert.That(await Support.StatusAsync(Ona, new DateTime(2026, 9, 1), 1), Is.EqualTo(TimePeriodStatus.Open));
        }

        [Test]
        public async Task LockAsync_FillsOpenMonthsAndBlocksChangesUntilUnlocked()
        {
            MakeAccountant(AsBoss);

            await _service.LockAsync(PastMonth, AsBoss);

            Assert.That(EntriesOf(Ona).Sum(entry => entry.Minutes), Is.EqualTo(22 * 480));
            Assert.ThrowsAsync<ValidationException>(() => _service.ApproveAsync(Ona, PastMonth, AsBoss));

            await _service.UnlockAsync(PastMonth, AsBoss);
            Assert.That(await Db.Set<TimeMonthLock>().AnyAsync(), Is.False);
        }

        [Test]
        public async Task LockAsync_FillsHoursAManagerRemovedFromASubmittedMonth()
        {
            MakeAccountant(AsBoss);
            await _service.SubmitAsync(PastMonth, AsOna);
            await new TimeTableService(Uow, Support).SaveEntriesAsync(PastMonth, Ona, new List<TimeEntryChangeDto>
            {
                new TimeEntryChangeDto { ProjectId = ShopId, TaskId = DevelopmentId, Date = "2026-09-01", Minutes = null }
            }, AsBoss);
            Assert.That(EntriesOf(Ona).Sum(entry => entry.Minutes), Is.EqualTo(21 * 480));

            await _service.LockAsync(PastMonth, AsBoss);

            Assert.That(EntriesOf(Ona).Sum(entry => entry.Minutes), Is.EqualTo(22 * 480));
            Assert.That(await Support.StatusAsync(Ona, new DateTime(2026, 9, 1), 1), Is.EqualTo(TimePeriodStatus.Submitted));
        }

        [Test]
        public void LockAsync_IsForAdministratorsOnly()
        {
            Assert.ThrowsAsync<UnauthorizedException>(() => _service.LockAsync(PastMonth, AsBoss));
        }

        [Test]
        public async Task GetTeamAsync_ListsDirectReportsWithStatus()
        {
            await _service.SubmitAsync(CurrentMonth, AsOna);

            var team = (await _service.GetTeamAsync(CurrentMonth, AsBoss)).ToList();

            Assert.That(team.Select(member => (member.Id, member.Status)), Is.EqualTo(new[] { (Ona, "submitted") }));
        }

        [Test]
        public async Task GetEmployeesAsync_ListsEveryActiveEmployeeExceptTheViewer()
        {
            MakeAccountant(AsBoss);
            var gone = User("gone", "Gone", "Away", null);
            gone.IsDeleted = true;
            Db.Set<ApplicationUser>().AddRange(User("petras", "Petras", "Petraitis", null), gone, User("newbie", "New", "Comer", null));
            Db.Set<ApplicationRole>().Add(new ApplicationRole { Id = "new-user", Name = Roles.NewUser, OrganizationId = 1 });
            Db.Set<IdentityUserRole<string>>().Add(new IdentityUserRole<string> { UserId = "newbie", RoleId = "new-user" });
            Db.SaveChanges(false);

            var employees = (await _service.GetEmployeesAsync(CurrentMonth, AsBoss)).ToList();

            Assert.That(employees.Select(member => member.Id), Is.EquivalentTo(new[] { Ona, Jonas, "petras" }));
            Assert.That(employees.Single(member => member.Id == Jonas).DailyMinutes, Is.EqualTo(360));
        }

        [Test]
        public async Task GetOverviewAsync_ShowsBadgesFromTwoWorkingDaysBeforeTheLastOne()
        {
            MakeAccountant(AsBoss);

            Today(new DateTime(2026, 10, 27));
            var early = await _service.GetOverviewAsync(AsBoss);
            Today(new DateTime(2026, 10, 28));
            var due = await _service.GetOverviewAsync(AsBoss);

            Assert.That((early.CanManage, early.ManageBadge, early.AdministrateBadge), Is.EqualTo((true, 0, 0)));
            Assert.That((due.ManageBadge, due.AdministrateBadge), Is.EqualTo((1, 2)));
        }

        [Test]
        public async Task GetOverviewAsync_CountsEmployeesWithoutProjectsForAdministrators()
        {
            MakeAccountant(AsBoss);
            Db.Set<ApplicationUser>().Add(User("petras", "Petras", "Petraitis", null));
            Db.SaveChanges(false);
            Today(new DateTime(2026, 10, 28));

            Assert.That((await _service.GetOverviewAsync(AsBoss)).AdministrateBadge, Is.EqualTo(3));
        }

        [Test]
        public async Task GetOverviewAsync_StartsEarlierWhenTheViewerIsAwayAtMonthEnd()
        {
            Db.Set<VacationRequest>().Add(new VacationRequest
            {
                OrganizationId = 1,
                EmployeeId = Boss,
                Type = VacationRequestType.Annual,
                Status = VacationRequestStatus.Pending,
                DateFrom = new DateTime(2026, 10, 26),
                DateTo = new DateTime(2026, 10, 30)
            });
            Db.SaveChanges(false);

            Today(new DateTime(2026, 10, 21));

            Assert.That((await _service.GetOverviewAsync(AsBoss)).ManageBadge, Is.EqualTo(1));
        }
    }
}
