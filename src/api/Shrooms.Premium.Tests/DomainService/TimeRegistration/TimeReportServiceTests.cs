using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Premium.Domain.Services.TimeRegistration;

namespace Shrooms.Premium.Tests.DomainService.TimeRegistration
{
    [TestFixture]
    public class TimeReportServiceTests : TimeRegistrationTestBase
    {
        private const int OctoberWorkingDays = 22;

        private TimeReportService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new TimeReportService(Uow, Support);
            MakeAccountant(AsBoss);
        }

        [Test]
        public async Task GetReportAsync_CountsEntriesAutoFillAndAbsences()
        {
            AddEntry(Ona, ShopId, ReviewId, new DateTime(2026, 10, 1), 120);
            Db.Set<VacationRequest>().Add(new VacationRequest
            {
                OrganizationId = 1,
                EmployeeId = Ona,
                Type = VacationRequestType.Annual,
                Status = VacationRequestStatus.Approved,
                DateFrom = new DateTime(2026, 10, 5),
                DateTo = new DateTime(2026, 10, 6)
            });
            Db.SaveChanges(false);

            var report = await _service.GetReportAsync(CurrentMonth, AsBoss);

            var ona = report.Employees.Single(employee => employee.Id == Ona);
            Assert.That(ona.Status, Is.EqualTo("open"));
            Assert.That(ona.ExpectedMinutes, Is.EqualTo(OctoberWorkingDays * 480));
            Assert.That(ona.LoggedMinutes, Is.EqualTo(OctoberWorkingDays * 480));
            Assert.That(ona.AbsenceMinutes, Is.EqualTo(2 * 480));
            Assert.That(
                report.Projects.Where(row => row.EmployeeId == Ona).Select(row => (row.TaskName ?? row.AbsenceType, row.Minutes)),
                Is.EquivalentTo(new[] { ("Review", 120), ("Development", ((OctoberWorkingDays - 2) * 480) - 120), ("annual", 2 * 480) }));

            var jonas = report.Employees.Single(employee => employee.Id == Jonas);
            Assert.That(jonas.ExpectedMinutes, Is.EqualTo(OctoberWorkingDays * 360));
            Assert.That(jonas.LoggedMinutes, Is.EqualTo(OctoberWorkingDays * 360));

            var boss = report.Employees.Single(employee => employee.Id == Boss);
            Assert.That(boss.LoggedMinutes, Is.Zero);
            Assert.That(boss.ManagerName, Is.Empty);
            Assert.That(ona.ManagerName, Is.EqualTo("Bea Boss"));
        }

        [Test]
        public async Task GetReportAsync_CountsOnlyStoredHoursOfAnApprovedMonth()
        {
            AddEntry(Ona, ShopId, ReviewId, new DateTime(2026, 10, 1), 120);
            Db.Set<TimePeriod>().Add(new TimePeriod { OrganizationId = 1, UserId = Ona, Month = new DateTime(2026, 10, 1), Status = TimePeriodStatus.Approved });
            Db.SaveChanges(false);

            var report = await _service.GetReportAsync(CurrentMonth, AsBoss);

            var ona = report.Employees.Single(employee => employee.Id == Ona);
            Assert.That(ona.Status, Is.EqualTo("approved"));
            Assert.That(ona.LoggedMinutes, Is.EqualTo(120));
        }

        [Test]
        public async Task GetReportAsync_SkipsAutoFillForAFutureMonth()
        {
            var report = await _service.GetReportAsync(FutureMonth, AsBoss);

            Assert.That(report.Employees.Select(employee => employee.LoggedMinutes), Is.All.Zero);
            Assert.That(report.Projects, Is.Empty);
        }

        [Test]
        public async Task GetReportAsync_SkipsAutoFillForALockedMonth()
        {
            Db.Set<TimeMonthLock>().Add(new TimeMonthLock { OrganizationId = 1, Month = new DateTime(2026, 10, 1), LockedById = Boss });
            Db.SaveChanges(false);

            var report = await _service.GetReportAsync(CurrentMonth, AsBoss);

            Assert.That(report.Employees.Single(employee => employee.Id == Jonas).LoggedMinutes, Is.Zero);
        }

        [Test]
        public async Task GetReportAsync_KeepsTheHoursOfADeletedEmployee()
        {
            var former = User("former", "Fred", "Former", Boss);
            former.IsDeleted = true;
            var gone = User("gone", "Gina", "Gone", Boss);
            gone.IsDeleted = true;
            Db.Set<ApplicationUser>().AddRange(former, gone);
            Db.SaveChanges(false);
            AddEntry("former", ShopId, DevelopmentId, new DateTime(2026, 10, 2), 300);

            var report = await _service.GetReportAsync(CurrentMonth, AsBoss);

            var row = report.Employees.Single(employee => employee.Id == "former");
            Assert.That(row.IsFormer, Is.True);
            Assert.That(row.FullName, Is.EqualTo("Fred Former"));
            Assert.That(row.LoggedMinutes, Is.EqualTo(300));
            Assert.That(row.ExpectedMinutes, Is.Null);
            Assert.That(report.Projects.Single(project => project.EmployeeId == "former").Minutes, Is.EqualTo(300));
            Assert.That(report.Employees.Any(employee => employee.Id == "gone"), Is.False);
            Assert.That(report.Employees.Where(employee => employee.Id != "former").All(employee => !employee.IsFormer), Is.True);
        }

        [Test]
        public void GetReportAsync_ForbidsSomeoneWhoIsNotAnAdministrator()
        {
            Assert.ThrowsAsync<UnauthorizedException>(() => _service.GetReportAsync(CurrentMonth, AsOna));
        }
    }
}
