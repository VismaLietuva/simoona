using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.DomainExceptions.Vacation;
using Shrooms.Premium.Domain.Services.Vacations;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class ParentalEntitlementServiceTests
    {
        private static readonly UserAndOrganizationDto Admin = new UserAndOrganizationDto { OrganizationId = 1, UserId = "admin" };

        private IUnitOfWork2 _uow;
        private DbSet<ParentalEntitlement> _entitlementDbSet;
        private ParentalEntitlementService _service;

        [SetUp]
        public void SetUp()
        {
            _uow = Substitute.For<IUnitOfWork2>();

            var users = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            users.SetDbSetDataForAsync(new List<ApplicationUser>
            {
                new ApplicationUser { Id = "ona", OrganizationId = 1, FirstName = "Ona", LastName = "Onaitė" },
                new ApplicationUser { Id = "jonas", OrganizationId = 1, FirstName = "Jonas", LastName = "Jonaitis" },
                new ApplicationUser { Id = "stranger", OrganizationId = 2, FirstName = "Other", LastName = "Tenant" }
            });
            _uow.GetDbSet<ApplicationUser>().Returns(users);

            _entitlementDbSet = Substitute.For<DbSet<ParentalEntitlement>, IQueryable<ParentalEntitlement>, IAsyncEnumerable<ParentalEntitlement>>();
            _entitlementDbSet.SetDbSetDataForAsync(new List<ParentalEntitlement>
            {
                new ParentalEntitlement { Id = 1, OrganizationId = 1, EmployeeId = "jonas", Type = ParentalEntitlementType.Hours2PerMonth }
            });
            _uow.GetDbSet<ParentalEntitlement>().Returns(_entitlementDbSet);

            var requests = Substitute.For<DbSet<VacationRequest>, IQueryable<VacationRequest>, IAsyncEnumerable<VacationRequest>>();
            requests.SetDbSetDataForAsync(new List<VacationRequest>());
            _uow.GetDbSet<VacationRequest>().Returns(requests);

            var organizations = Substitute.For<DbSet<Organization>, IQueryable<Organization>, IAsyncEnumerable<Organization>>();
            organizations.SetDbSetDataForAsync(new List<Organization> { new Organization { Id = 1 } });
            _uow.GetDbSet<Organization>().Returns(organizations);

            _service = new ParentalEntitlementService(_uow);
        }

        [Test]
        public async Task GetAllAsync_ListsEveryEmployeeOfTheOrganisation_AssignedOrNot()
        {
            var rows = await _service.GetAllAsync(new ParentalEntitlementListArgsDto { OrganizationId = 1 });

            Assert.That(rows.Select(row => row.Employee.Id), Is.EqualTo(new[] { "jonas", "ona" }));
            Assert.That(rows.Single(row => row.Employee.Id == "jonas").Type, Is.EqualTo("hours2PerMonth"));
            Assert.That(rows.Single(row => row.Employee.Id == "ona").Type, Is.Null);
        }

        [Test]
        public async Task SetAsync_AddsAnEntitlement()
        {
            var row = await _service.SetAsync("ona", "dayPerMonth", Admin);

            Assert.That(row.Type, Is.EqualTo("dayPerMonth"));
            _entitlementDbSet.Received(1).Add(Arg.Is<ParentalEntitlement>(e =>
                e.EmployeeId == "ona" && e.OrganizationId == 1 && e.Type == ParentalEntitlementType.DayPerMonth));
            await _uow.Received(1).SaveChangesAsync("admin");
        }

        [Test]
        public async Task SetAsync_OverwritesTheExistingRow()
        {
            var row = await _service.SetAsync("jonas", "twoDaysPerMonth", Admin);

            Assert.That(row.Type, Is.EqualTo("twoDaysPerMonth"));
            _entitlementDbSet.DidNotReceive().Add(Arg.Any<ParentalEntitlement>());
            await _uow.Received(1).SaveChangesAsync("admin");
        }

        [Test]
        public async Task SetAsync_RemovesTheRowForNone()
        {
            var row = await _service.SetAsync("jonas", "none", Admin);

            Assert.That(row.Type, Is.Null);
            _entitlementDbSet.Received(1).Remove(Arg.Is<ParentalEntitlement>(e => e.EmployeeId == "jonas"));
        }

        [Test]
        public void SetAsync_RefusesAnUnknownType()
        {
            var error = Assert.ThrowsAsync<VacationValidationException>(() => _service.SetAsync("ona", "fiveDaysPerWeek", Admin));

            Assert.That(error.Code, Is.EqualTo("parentalTypeInvalid"));
        }

        [Test]
        public void SetAsync_CannotReachAnEmployeeOfAnotherOrganisation()
        {
            var error = Assert.ThrowsAsync<VacationValidationException>(() => _service.SetAsync("stranger", "dayPerMonth", Admin));

            Assert.That(error.Code, Is.EqualTo("notFound"));
            _entitlementDbSet.DidNotReceive().Add(Arg.Any<ParentalEntitlement>());
        }

        [Test]
        public async Task GetBalanceAsync_IsEmptyWithNoEntitlement()
        {
            var balance = await _service.GetBalanceAsync(new UserAndOrganizationDto { OrganizationId = 1, UserId = "ona" });

            Assert.That(balance.Type, Is.Null);
            Assert.That(balance.Booked, Is.Null);
        }

        [Test]
        public async Task GetBalanceAsync_HasNoBookingFigureForShorterHours()
        {
            var balance = await _service.GetBalanceAsync(new UserAndOrganizationDto { OrganizationId = 1, UserId = "jonas" });

            Assert.That(balance.Type, Is.EqualTo("hours2PerMonth"));
            Assert.That(balance.Unit, Is.EqualTo("hours"));
            Assert.That(balance.Period, Is.EqualTo("month"));
            Assert.That(balance.PeriodFrom, Does.EndWith("-01"));
            Assert.That(balance.Booked, Is.Null);
        }
    }
}
