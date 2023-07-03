using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Employees;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using System.Data.Entity;
using Shrooms.Tests.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Employees;
using Shrooms.Domain.Helpers;
using System.Linq;
using Shrooms.Contracts.Constants;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class EmployeeListingServiceTests
    {
        private EmployeeListingService _employeeListingService;

        private DbSet<ApplicationUser> _usersDbSet;
        private IRoleService _roleService;
        private IPermissionService _permissionService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = uow.MockDbSetForAsync<ApplicationUser>();

            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _roleService = Substitute.For<IRoleService>();
            _permissionService = Substitute.For<IPermissionService>();

            _employeeListingService = new EmployeeListingService(
                uow,
                _permissionService,
                _roleService);
        }

        [Test]
        public async Task GetPagedEmployeesAsync_WhenUserIsNotAdmin_HidesPrivateInformation()
        {
            // Arrange
            var birthDay = DateTime.UtcNow;
            var expectedBirthdayValue = BirthdayDateTimeHelper.RemoveYear(birthDay);

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var employees = GetTestDataForGetPagedEmployeesAsync();

            foreach (var employee in employees)
            {
                employee.BirthDay = birthDay;
            }

            _usersDbSet.SetDbSetDataForAsync(employees);

            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .Returns(false);

            _roleService
                .ExcludeUsersWithRole(Arg.Any<string>())
                .Returns(value => true);

            var args = new EmployeeListingArgsDto
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _employeeListingService.GetPagedEmployeesAsync(args, userOrg);

            // Assert
            Assert.That(result, Is.All.Matches<EmployeeDto>(employee => employee.BlacklistEntry == null));
            Assert.That(result, Is.All.Matches<EmployeeDto>(employee => employee.PhoneNumber == null));
            Assert.That(result, Is.All.Matches<EmployeeDto>(employee => employee.BirthDay == expectedBirthdayValue));
        }


        [Test]
        public async Task GetPagedEmployeesAsync_WhenSearchStringIsGiven_ReturnsFilteredEmployees()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var employees = GetTestDataForGetPagedEmployeesAsync();

            var expectedEmployeeIds = employees
                .Where(employee => employee.FirstName == "Anton")
                .Select(employee => employee.Id);

            _usersDbSet.SetDbSetDataForAsync(employees);

            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .Returns(true);

            _roleService
                .ExcludeUsersWithRole(Arg.Any<string>())
                .Returns(value => true);

            var args = new EmployeeListingArgsDto
            {
                Search = "Anton",
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _employeeListingService.GetPagedEmployeesAsync(args, userOrg);

            // Assert
            Assert.That(result, Is.All.Matches<EmployeeDto>(employee => expectedEmployeeIds.Contains(employee.Id)));
        }

        [Test]
        public async Task GetPagedEmployeesAsync_WhenSearchStringIsNotProvided_ReturnsNotFilteredEmployees()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var employees = GetTestDataForGetPagedEmployeesAsync();
            var expectedIds = employees.Select(employee => employee.Id);

            _usersDbSet.SetDbSetDataForAsync(employees);

            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .Returns(true);

            _roleService
                .ExcludeUsersWithRole(Arg.Any<string>())
                .Returns(value => true);

            var args = new EmployeeListingArgsDto
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _employeeListingService.GetPagedEmployeesAsync(args, userOrg);

            // Assert
            Assert.That(result, Is.All.Matches<EmployeeDto>(employee => expectedIds.Contains(employee.Id)));
        }

        [Test]
        public async Task GetPagedEmployeesAsync_WhenOnlyShowBlacklistedIsTrue_ReturnsOnlyBlacklistedEmployees()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _usersDbSet.SetDbSetDataForAsync(GetTestDataForGetPagedEmployeesAsync());

            _permissionService
                .GetUserPermissionsAsync(Arg.Any<string>(), Arg.Any<int>())
                .Returns(new List<string> { BasicPermissions.Blacklist });

            _roleService
                .ExcludeUsersWithRole(Arg.Any<string>())
                .Returns(value => true);

            var args = new EmployeeListingArgsDto
            {
                ShowOnlyBlacklisted = true,
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _employeeListingService.GetPagedEmployeesAsync(args, userOrg);

            // Assert
            Assert.That(result, Is.All.Matches<EmployeeDto>(employee => employee.BlacklistEntry != null));
        }

        [Test]
        public async Task GetPagedEmployeesAsync_WhenOnlyShowBlacklistedIsTrueAndUserIsNotAdmin_ReturnsAllEmployees()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var employees = GetTestDataForGetPagedEmployeesAsync();
            var expectedEmployeeIds = employees.Select(employee => employee.Id);

            _usersDbSet.SetDbSetDataForAsync(employees);

            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .Returns(false);

            _roleService
                .ExcludeUsersWithRole(Arg.Any<string>())
                .Returns(value => true);

            var args = new EmployeeListingArgsDto
            {
                ShowOnlyBlacklisted = true,
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _employeeListingService.GetPagedEmployeesAsync(args, userOrg);

            // Assert
            Assert.That(result, Is.All.Matches<EmployeeDto>(employee => expectedEmployeeIds.Contains(employee.Id)));
        }

        [Test]
        public async Task GetPagedEmployeesAsync_ReturnsSortedEmployees()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var employees = GetTestDataForGetPagedEmployeesAsync();

            var expectedEmployeeIdsOrder = employees
                .OrderByDescending(employee => employee.BirthDay)
                .Select(employee => employee.Id);

            _usersDbSet.SetDbSetDataForAsync(employees);

            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .Returns(false);

            _roleService
                .ExcludeUsersWithRole(Arg.Any<string>())
                .Returns(value => true);

            var args = new EmployeeListingArgsDto
            {
                SortByProperties = "BirthDay desc;",
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _employeeListingService.GetPagedEmployeesAsync(args, userOrg);

            // Assert
            CollectionAssert.AreEqual(expectedEmployeeIdsOrder, result.Select(employee => employee.Id));
        }

        private IList<ApplicationUser> GetTestDataForGetPagedEmployeesAsync()
        {
            return new List<ApplicationUser>
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    OrganizationId = 1,
                    FirstName = "David",
                    LastName = "Peterson",
                    JobPosition = new JobPosition
                    {
                        Title = "Awesome job"
                    },
                    BirthDay = DateTime.UtcNow.AddDays(3),
                    PhoneNumber = "+370600000000",
                    WorkingHours = new WorkingHours
                    {
                        StartTime = new TimeSpan(),
                        EndTime = new TimeSpan()
                    },
                    BlacklistEntries = new List<BlacklistUser>()
                },

                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    OrganizationId = 1,
                    FirstName = "Anton",
                    LastName = "Peterson",
                    JobPosition = new JobPosition
                    {
                        Title = "Awesome job"
                    },
                    BirthDay = DateTime.UtcNow.AddDays(2),
                    PhoneNumber = "+370600000000",
                    WorkingHours = new WorkingHours
                    {
                        StartTime = new TimeSpan(),
                        EndTime = new TimeSpan()
                    },
                    BlacklistEntries = new List<BlacklistUser>()
                },

                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    OrganizationId = 1,
                    FirstName = "Borat",
                    LastName = "Peterson",
                    JobPosition = new JobPosition
                    {
                        Title = "Awesome job"
                    },
                    BirthDay = DateTime.UtcNow.AddDays(1),
                    PhoneNumber = "+370600000000",
                    WorkingHours = new WorkingHours
                    {
                        StartTime = new TimeSpan(),
                        EndTime = new TimeSpan()
                    },
                    BlacklistEntries = new List<BlacklistUser>
                    {
                        new()
                        {
                            EndDate = DateTime.MaxValue
                        }
                    }
                }
            };
        }
    }
}
