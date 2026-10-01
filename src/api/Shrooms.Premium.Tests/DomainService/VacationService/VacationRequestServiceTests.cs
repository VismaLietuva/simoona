using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.DomainExceptions.Vacation;
using Shrooms.Premium.Domain.Services.Email.Vacations;
using Shrooms.Premium.Domain.Services.Vacations;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class VacationRequestServiceTests
    {
        private static readonly UserAndOrganizationDto Employee = new UserAndOrganizationDto { OrganizationId = 1, UserId = "ona" };

        private IUnitOfWork2 _uow;
        private VacationRequest _request;
        private VacationRequestService _service;

        [SetUp]
        public void SetUp()
        {
            _uow = Substitute.For<IUnitOfWork2>();

            _request = new VacationRequest
            {
                Id = 1,
                OrganizationId = 1,
                EmployeeId = "ona",
                Employee = new ApplicationUser { Id = "ona", OrganizationId = 1, FirstName = "Ona", LastName = "Onaitė" },
                Type = VacationRequestType.Parental,
                Status = VacationRequestStatus.Pending,
                DateFrom = NextMonday(),
                DateTo = NextMonday(),
                WorkingDays = 1
            };

            var requests = Substitute.For<DbSet<VacationRequest>, IQueryable<VacationRequest>, IAsyncEnumerable<VacationRequest>>();
            requests.SetDbSetDataForAsync(new List<VacationRequest> { _request });
            _uow.GetDbSet<VacationRequest>().Returns(requests);

            var events = Substitute.For<DbSet<VacationRequestEvent>, IQueryable<VacationRequestEvent>, IAsyncEnumerable<VacationRequestEvent>>();
            events.SetDbSetDataForAsync(new List<VacationRequestEvent>());
            _uow.GetDbSet<VacationRequestEvent>().Returns(events);

            var users = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            users.SetDbSetDataForAsync(new List<ApplicationUser> { _request.Employee });
            _uow.GetDbSet<ApplicationUser>().Returns(users);

            var organizations = Substitute.For<DbSet<Organization>, IQueryable<Organization>, IAsyncEnumerable<Organization>>();
            organizations.SetDbSetDataForAsync(new List<Organization> { new Organization { Id = 1 } });
            _uow.GetDbSet<Organization>().Returns(organizations);

            var entitlements = Substitute.For<DbSet<ParentalEntitlement>, IQueryable<ParentalEntitlement>, IAsyncEnumerable<ParentalEntitlement>>();
            entitlements.SetDbSetDataForAsync(new List<ParentalEntitlement>());
            _uow.GetDbSet<ParentalEntitlement>().Returns(entitlements);

            var holidays = Substitute.For<IHolidayService>();
            holidays.GetCalendarAsync().Returns(HolidayCalendar.Empty);

            _service = new VacationRequestService(
                _uow,
                holidays,
                Substitute.For<IVacationNotificationService>(),
                Substitute.For<ILogger<VacationRequestService>>());
        }

        [Test]
        public async Task EditAsync_SavesANoteOnlyChangeToAParentalRequestWithoutAnEntitlement()
        {
            var edited = await _service.EditAsync(1, Draft(_request.DateFrom, "Doctor's appointment"), Employee);

            Assert.That(edited.Note, Is.EqualTo("Doctor's appointment"));
            await _uow.Received(1).SaveChangesAsync("ona");
        }

        [Test]
        public void EditAsync_StillChecksTheEntitlementWhenTheDateMoves()
        {
            var error = Assert.ThrowsAsync<VacationValidationException>(() =>
                _service.EditAsync(1, Draft(_request.DateFrom.AddDays(1), null), Employee));

            Assert.That(error.Code, Is.EqualTo("parentalNotEntitled"));
        }

        private static VacationRequestDraftDto Draft(DateTime day, string note)
        {
            return new VacationRequestDraftDto
            {
                Type = "parental",
                DateFrom = VacationWireFormat.ToDay(day),
                DateTo = VacationWireFormat.ToDay(day),
                Note = note
            };
        }

        private static DateTime NextMonday()
        {
            var day = DateTime.Today.AddDays(14);
            return day.AddDays(((int)DayOfWeek.Monday - (int)day.DayOfWeek + 7) % 7);
        }
    }
}
