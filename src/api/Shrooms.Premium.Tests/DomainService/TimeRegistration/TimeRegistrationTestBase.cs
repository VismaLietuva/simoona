using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.TimeRegistration;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Premium.Domain.Services.TimeRegistration;
using Shrooms.Premium.Domain.Services.Vacations;

namespace Shrooms.Premium.Tests.DomainService.TimeRegistration
{
    public abstract class TimeRegistrationTestBase
    {
        protected const string Boss = "boss";
        protected const string Ona = "ona";
        protected const string Jonas = "jonas";
        protected const string CurrentMonth = "2026-10";
        protected const string PastMonth = "2026-09";
        protected const string FutureMonth = "2026-11";
        protected const int ShopId = 1;
        protected const int DevelopmentId = 1;
        protected const int ReviewId = 2;
        protected const int OpsId = 2;
        protected const int OpsTaskId = 3;

        protected static readonly UserAndOrganizationDto AsBoss = new UserAndOrganizationDto { OrganizationId = 1, UserId = Boss };
        protected static readonly UserAndOrganizationDto AsOna = new UserAndOrganizationDto { OrganizationId = 1, UserId = Ona };
        protected static readonly UserAndOrganizationDto AsJonas = new UserAndOrganizationDto { OrganizationId = 1, UserId = Jonas };

        protected ShroomsDbContext Db { get; private set; }

        protected UnitOfWork2 Uow { get; private set; }

        protected IPermissionService Permissions { get; private set; }

        protected IHolidayService Holidays { get; private set; }

        protected ISystemClock Clock { get; private set; }

        protected TimeRegistrationSupport Support { get; private set; }

        [SetUp]
        public void SetUpDatabase()
        {
            Db = new ShroomsDbContext(new DbContextOptionsBuilder<ShroomsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
            Uow = new UnitOfWork2(Db);

            Db.Set<Organization>().Add(new Organization { Id = 1, ShortName = "Test", Name = "TestOrg", WelcomeEmail = "Welcome" });
            var jonas = User(Jonas, "Jonas", "Jonaitis", null);
            jonas.WorkingHours = new WorkingHours { OrganizationId = 1, FullTime = false, PartTimeHours = 6 };
            Db.Set<ApplicationUser>().AddRange(
                User(Boss, "Bea", "Boss", null),
                User(Ona, "Ona", "Onaitė", Boss),
                jonas);
            Db.SaveChanges(false);

            Db.Set<TimeCustomer>().AddRange(
                new TimeCustomer { Id = 1, OrganizationId = 1, Name = "Northwind" },
                new TimeCustomer { Id = 2, OrganizationId = 1, Name = "Contoso" });
            Db.Set<TimeProject>().AddRange(
                new TimeProject { Id = ShopId, OrganizationId = 1, CustomerId = 1, Name = "Shop" },
                new TimeProject { Id = OpsId, OrganizationId = 1, CustomerId = 2, Name = "Ops" });
            Db.Set<TimeTask>().AddRange(
                new TimeTask { Id = DevelopmentId, OrganizationId = 1, ProjectId = ShopId, Name = "Development", Comment = string.Empty, IsDefault = true },
                new TimeTask { Id = ReviewId, OrganizationId = 1, ProjectId = ShopId, Name = "Review", Comment = string.Empty },
                new TimeTask { Id = OpsTaskId, OrganizationId = 1, ProjectId = OpsId, Name = "Support", Comment = string.Empty, IsDefault = true });
            Db.Set<TimeProjectMember>().AddRange(
                new TimeProjectMember { OrganizationId = 1, ProjectId = ShopId, UserId = Ona },
                new TimeProjectMember { OrganizationId = 1, ProjectId = ShopId, UserId = Jonas });
            Db.SaveChanges(false);

            Permissions = Substitute.For<IPermissionService>();
            Holidays = Substitute.For<IHolidayService>();
            Holidays.GetAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>()).Returns(new List<Holiday>());
            Clock = Substitute.For<ISystemClock>();
            Today(new DateTime(2026, 10, 15));

            Support = new TimeRegistrationSupport(Uow, Permissions, new RoleService(Uow, Permissions), Holidays, Clock);
        }

        [TearDown]
        public void TearDownDatabase()
        {
            Db.Dispose();
        }

        protected void Today(DateTime day)
        {
            Clock.UtcNow.Returns(day.Date.AddHours(9));
        }

        protected void MakeAccountant(UserAndOrganizationDto user)
        {
            Permissions
                .UserHasPermissionAsync(Arg.Is<UserAndOrganizationDto>(dto => dto.UserId == user.UserId), AdministrationPermissions.TimeRegistration)
                .Returns(true);
        }

        protected void AddEntry(string userId, int projectId, int taskId, DateTime date, int minutes)
        {
            Db.Set<TimeEntry>().Add(new TimeEntry { OrganizationId = 1, UserId = userId, ProjectId = projectId, TaskId = taskId, Date = date, Minutes = minutes });
            Db.SaveChanges(false);
        }

        protected List<TimeEntry> EntriesOf(string userId)
        {
            return Db.Set<TimeEntry>().AsNoTracking().Where(entry => entry.UserId == userId).OrderBy(entry => entry.Date).ToList();
        }

        protected static ApplicationUser User(string id, string firstName, string lastName, string managerId)
        {
            return new ApplicationUser
            {
                Id = id,
                OrganizationId = 1,
                UserName = id,
                Email = $"{id}@example.com",
                FirstName = firstName,
                LastName = lastName,
                ManagerId = managerId
            };
        }
    }
}
