using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Domain.Exceptions.Exceptions;
using Shrooms.Premium.DataTransferObjects.Models.TimeRegistration;
using Shrooms.Premium.Domain.Services.TimeRegistration;

namespace Shrooms.Premium.Tests.DomainService.TimeRegistration
{
    [TestFixture]
    public class TimeTableServiceTests : TimeRegistrationTestBase
    {
        private TimeTableService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new TimeTableService(Uow, Support);
        }

        [Test]
        public async Task GetTimeTableAsync_MarksHolidaysAbsencesAndPartTimeHours()
        {
            Holidays.GetAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
                .Returns(new List<Holiday> { new Holiday { Date = new DateTime(2026, 11, 2), Name = "Vėlinės" } });
            Db.Set<VacationRequest>().Add(new VacationRequest
            {
                OrganizationId = 1,
                EmployeeId = Jonas,
                Type = VacationRequestType.Annual,
                Status = VacationRequestStatus.Pending,
                DateFrom = new DateTime(2026, 11, 5),
                DateTo = new DateTime(2026, 11, 9)
            });
            Db.SaveChanges(false);

            var table = await _service.GetTimeTableAsync(FutureMonth, null, AsJonas);

            Assert.That(table.DailyMinutes, Is.EqualTo(360));
            Assert.That(table.Days.Single(day => day.Date == "2026-11-01").Holiday, Is.EqualTo("Visų šventųjų diena"));
            Assert.That(table.Days.Single(day => day.Date == "2026-11-02").ExpectedMinutes, Is.EqualTo(0));
            Assert.That(table.Days.Single(day => day.Date == "2026-11-03").ExpectedMinutes, Is.EqualTo(360));
            Assert.That(table.Absences.Select(absence => absence.Date), Is.EqualTo(new[] { "2026-11-05", "2026-11-06", "2026-11-09" }));
            Assert.That(table.Absences.All(absence => absence.Type == "annual" && absence.Status == "pending"), Is.True);
            Assert.That(table.Projects.Select(project => project.Name), Is.EqualTo(new[] { "Shop" }));
            Assert.That(table.AvailableProjects.Select(project => project.Name), Is.EqualTo(new[] { "Ops" }));
            Assert.That(table.AutoFillProjectId, Is.EqualTo(ShopId));
            Assert.That(table.Status, Is.EqualTo("open"));
        }

        [Test]
        public void GetTimeTableAsync_ForbidsSomeoneElsesTableToAColleague()
        {
            Assert.ThrowsAsync<UnauthorizedException>(() => _service.GetTimeTableAsync(CurrentMonth, Ona, AsJonas));
        }

        [Test]
        public async Task GetTimeTableAsync_LetsAManagerOpenTheirReportsTable()
        {
            var table = await _service.GetTimeTableAsync(CurrentMonth, Ona, AsBoss);

            Assert.That(table.Projects.Single().IsAssigned, Is.True);
        }

        [Test]
        public async Task SaveEntriesAsync_UpsertsAndClearsEntries()
        {
            AddEntry(Ona, ShopId, ReviewId, new DateTime(2026, 10, 2), 60);

            await _service.SaveEntriesAsync(CurrentMonth, null, new List<TimeEntryChangeDto>
            {
                new TimeEntryChangeDto { ProjectId = ShopId, TaskId = ReviewId, Date = "2026-10-01", Minutes = 90 },
                new TimeEntryChangeDto { ProjectId = ShopId, TaskId = ReviewId, Date = "2026-10-02", Minutes = null }
            }, AsOna);

            var entries = EntriesOf(Ona);
            Assert.That(entries.Select(entry => (entry.Date.Day, entry.Minutes)), Is.EqualTo(new[] { (1, 90) }));
        }

        [Test]
        public void SaveEntriesAsync_RejectsAnUnassignedProject()
        {
            var error = Assert.ThrowsAsync<ValidationException>(() => _service.SaveEntriesAsync(CurrentMonth, null, new List<TimeEntryChangeDto>
            {
                new TimeEntryChangeDto { ProjectId = OpsId, TaskId = OpsTaskId, Date = "2026-10-01", Minutes = 60 }
            }, AsOna));

            Assert.That(error.ErrorMessage, Is.EqualTo("Project \"Ops\" is not assigned"));
        }

        [Test]
        public void SaveEntriesAsync_RejectsMoreThanADayOfHours()
        {
            AddEntry(Ona, ShopId, ReviewId, new DateTime(2026, 10, 1), 1000);

            var error = Assert.ThrowsAsync<ValidationException>(() => _service.SaveEntriesAsync(CurrentMonth, null, new List<TimeEntryChangeDto>
            {
                new TimeEntryChangeDto { ProjectId = ShopId, TaskId = DevelopmentId, Date = "2026-10-01", Minutes = 500 }
            }, AsOna));

            Assert.That(error.ErrorMessage, Is.EqualTo("More than 24 hours on 2026-10-01"));
        }

        [Test]
        public void SaveEntriesAsync_KeepsASubmittedMonthReadOnlyForTheEmployee()
        {
            Db.Set<TimePeriod>().Add(new TimePeriod { OrganizationId = 1, UserId = Ona, Month = new DateTime(2026, 10, 1), Status = TimePeriodStatus.Submitted });
            Db.SaveChanges(false);

            var error = Assert.ThrowsAsync<ValidationException>(() => _service.SaveEntriesAsync(CurrentMonth, null, OneHour(), AsOna));

            Assert.That(error.ErrorCode, Is.EqualTo(ErrorCodes.TimeRegistrationNotEditable));
        }

        [Test]
        public async Task SaveEntriesAsync_LetsTheManagerEditASubmittedMonth()
        {
            Db.Set<TimePeriod>().Add(new TimePeriod { OrganizationId = 1, UserId = Ona, Month = new DateTime(2026, 10, 1), Status = TimePeriodStatus.Submitted });
            Db.SaveChanges(false);

            await _service.SaveEntriesAsync(CurrentMonth, Ona, OneHour(), AsBoss);

            Assert.That(EntriesOf(Ona), Has.Count.EqualTo(1));
        }

        [Test]
        public void SaveEntriesAsync_RefusesALockedMonth()
        {
            Db.Set<TimeMonthLock>().Add(new TimeMonthLock { OrganizationId = 1, Month = new DateTime(2026, 10, 1), LockedById = Boss });
            Db.SaveChanges(false);

            var error = Assert.ThrowsAsync<ValidationException>(() => _service.SaveEntriesAsync(CurrentMonth, null, OneHour(), AsOna));

            Assert.That(error.ErrorCode, Is.EqualTo(ErrorCodes.TimeRegistrationLocked));
        }

        [Test]
        public async Task AddProjectAsync_AssignsTheProjectAndSetAutoFillMovesTheChoice()
        {
            await _service.AddProjectAsync(OpsId, null, AsOna);
            await _service.SetAutoFillAsync(OpsId, null, AsOna);

            Assert.That(await Support.AutoFillProjectIdAsync(Ona, 1), Is.EqualTo(OpsId));
        }

        private static List<TimeEntryChangeDto> OneHour()
        {
            return new List<TimeEntryChangeDto>
            {
                new TimeEntryChangeDto { ProjectId = ShopId, TaskId = ReviewId, Date = "2026-10-01", Minutes = 60 }
            };
        }
    }
}
