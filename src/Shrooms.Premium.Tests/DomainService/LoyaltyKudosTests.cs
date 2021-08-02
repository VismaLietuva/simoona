using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Premium.Domain.Services.WebHookCallbacks.LoyaltyKudos;
using Shrooms.Premium.Tests.ModelMappings;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.DomainService
{
    public class LoyaltyKudosTests
    {
        private ILoyaltyKudosService _loyaltyKudosService;
        private ILoyaltyKudosCalculator _loyaltyKudosCalculator;

        private DbSet<KudosLog> _kudosLogsDbSet;
        private DbSet<KudosType> _kudosTypeDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<Organization> _organizationsDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _kudosLogsDbSet = uow.MockDbSetForAsync<KudosLog>();
            _kudosTypeDbSet = uow.MockDbSetForAsync<KudosType>();
            _usersDbSet = uow.MockDbSetForAsync<ApplicationUser>();
            _organizationsDbSet = uow.MockDbSetForAsync<Organization>();

            var loggerMock = Substitute.For<ILogger>();
            var asyncRunner = Substitute.For<IAsyncRunner>();
            var mapper = ModelMapper.Create();

            _loyaltyKudosCalculator = new LoyaltyKudosCalculator();
            _loyaltyKudosService = new LoyaltyKudosService(uow, loggerMock, asyncRunner, mapper, _loyaltyKudosCalculator);
        }

        [Test]
        public void Should_Award_1_Year_Employee_With_Loyalty_Kudos()
        {
            // Setup
            const int organizationId = 2;
            const int yearOfEmployment = 1;
            var kudosYearlyMultipliers = new[] { 2, 2, 4, 4 };
            var oneYearEmployee = new ApplicationUser
            {
                Id = "user",
                OrganizationId = 2,
                RemainingKudos = 0,
                TotalKudos = 10,
                EmploymentDate = DateTime.Now.AddYears(-2)
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10,
                Name = "Loyalty"
            };

            // Act
            var loyaltyKudosLog = _loyaltyKudosCalculator.CreateLoyaltyKudosLog(oneYearEmployee, loyaltyKudosType, organizationId,
                kudosYearlyMultipliers, yearOfEmployment);

            Assert.AreEqual(oneYearEmployee.Id, loyaltyKudosLog.EmployeeId);
            Assert.AreEqual(20, loyaltyKudosLog.Points);
            Assert.AreEqual(2, loyaltyKudosLog.MultiplyBy);
            Assert.AreEqual(KudosStatus.Approved, loyaltyKudosLog.Status);
            Assert.AreEqual(loyaltyKudosType.Value, loyaltyKudosLog.KudosTypeValue);
            Assert.AreEqual(loyaltyKudosType.Name, loyaltyKudosLog.KudosTypeName);
            Assert.AreEqual("KudosLoyaltyBot", loyaltyKudosLog.CreatedBy);
            Assert.AreEqual("Kudos for 1 year loyalty", loyaltyKudosLog.Comments);
            Assert.AreEqual(20, oneYearEmployee.RemainingKudos);
            Assert.AreEqual(30, oneYearEmployee.TotalKudos);
        }

        [Test]
        public void Should_Award_2_Year_Employee_With_Loyalty_Kudos()
        {
            // Setup
            const int organizationId = 2;
            const int yearOfEmployment = 2;
            var kudosYearlyMultipliers = new[] { 1, 2, 3, 4 };
            var twoYearEmployee = new ApplicationUser
            {
                Id = "user",
                OrganizationId = 2,
                RemainingKudos = 0,
                TotalKudos = 10,
                EmploymentDate = DateTime.Now.AddYears(-2)
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10,
                Name = "Loyalty"
            };

            // Act
            var loyaltyKudosLog = _loyaltyKudosCalculator.CreateLoyaltyKudosLog(twoYearEmployee, loyaltyKudosType, organizationId,
                kudosYearlyMultipliers, yearOfEmployment);

            // Assert
            Assert.AreEqual(twoYearEmployee.Id, loyaltyKudosLog.EmployeeId);
            Assert.AreEqual(20, loyaltyKudosLog.Points);
            Assert.AreEqual(2, loyaltyKudosLog.MultiplyBy);
            Assert.AreEqual(KudosStatus.Approved, loyaltyKudosLog.Status);
            Assert.AreEqual(loyaltyKudosType.Value, loyaltyKudosLog.KudosTypeValue);
            Assert.AreEqual(loyaltyKudosType.Name, loyaltyKudosLog.KudosTypeName);
            Assert.AreEqual("KudosLoyaltyBot", loyaltyKudosLog.CreatedBy);
            Assert.AreEqual("Kudos for 2 year loyalty", loyaltyKudosLog.Comments);
            Assert.AreEqual(20, twoYearEmployee.RemainingKudos);
            Assert.AreEqual(30, twoYearEmployee.TotalKudos);
        }

        [Test]
        public void Should_Award_3_Year_Employee_With_Loyalty_Kudos()
        {
            // Setup
            const int organizationId = 2;
            const int yearOfEmployment = 3;
            var kudosYearlyMultipliers = new[] { 1, 2, 4, 4 };
            var threeYearEmployee = new ApplicationUser
            {
                Id = "user",
                OrganizationId = 2,
                RemainingKudos = 0,
                TotalKudos = 10,
                EmploymentDate = DateTime.Now.AddYears(-2)
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10,
                Name = "Loyalty"
            };

            // Act
            var loyaltyKudosLog = _loyaltyKudosCalculator.CreateLoyaltyKudosLog(threeYearEmployee, loyaltyKudosType, organizationId,
                kudosYearlyMultipliers, yearOfEmployment);

            // Assert
            Assert.AreEqual(threeYearEmployee.Id, loyaltyKudosLog.EmployeeId);
            Assert.AreEqual(40, loyaltyKudosLog.Points);
            Assert.AreEqual(4, loyaltyKudosLog.MultiplyBy);
            Assert.AreEqual(KudosStatus.Approved, loyaltyKudosLog.Status);
            Assert.AreEqual(loyaltyKudosType.Value, loyaltyKudosLog.KudosTypeValue);
            Assert.AreEqual(loyaltyKudosType.Name, loyaltyKudosLog.KudosTypeName);
            Assert.AreEqual("KudosLoyaltyBot", loyaltyKudosLog.CreatedBy);
            Assert.AreEqual("Kudos for 3 year loyalty", loyaltyKudosLog.Comments);
            Assert.AreEqual(40, threeYearEmployee.RemainingKudos);
            Assert.AreEqual(50, threeYearEmployee.TotalKudos);
        }

        [Test]
        public void Should_Award_10_Year_Employee_With_Loyalty_Kudos()
        {
            // Setup
            const int organizationId = 2;
            const int yearOfEmployment = 10;
            var kudosYearlyMultipliers = new[] { 1, 2, 4, 4 };
            var threeYearEmployee = new ApplicationUser
            {
                Id = "user",
                OrganizationId = 2,
                RemainingKudos = 0,
                TotalKudos = 10,
                EmploymentDate = DateTime.Now.AddYears(-2)
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10,
                Name = "Loyalty"
            };

            // Act
            var loyaltyKudosLog = _loyaltyKudosCalculator.CreateLoyaltyKudosLog(threeYearEmployee, loyaltyKudosType, organizationId,
                kudosYearlyMultipliers, yearOfEmployment);

            // Assert
            Assert.AreEqual(threeYearEmployee.Id, loyaltyKudosLog.EmployeeId);
            Assert.AreEqual(40, loyaltyKudosLog.Points);
            Assert.AreEqual(4, loyaltyKudosLog.MultiplyBy);
            Assert.AreEqual(KudosStatus.Approved, loyaltyKudosLog.Status);
            Assert.AreEqual(loyaltyKudosType.Value, loyaltyKudosLog.KudosTypeValue);
            Assert.AreEqual(loyaltyKudosType.Name, loyaltyKudosLog.KudosTypeName);
            Assert.AreEqual("KudosLoyaltyBot", loyaltyKudosLog.CreatedBy);
            Assert.AreEqual("Kudos for 10 year loyalty", loyaltyKudosLog.Comments);
            Assert.AreEqual(40, threeYearEmployee.RemainingKudos);
            Assert.AreEqual(50, threeYearEmployee.TotalKudos);
        }

        [Test]
        public void Should_Not_Award_Employee_With_Loyalty_Kudos_When_Multiplier_Is_Zero()
        {
            // Setup
            const int organizationId = 2;
            const int yearOfEmployment = 2;
            var kudosYearlyMultipliers = new[] { 0, 0, 3, 4 };
            var twoYearEmployee = new ApplicationUser
            {
                Id = "user",
                OrganizationId = 2,
                RemainingKudos = 0,
                TotalKudos = 10
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10
            };

            // Act
            var loyaltyKudosLog = _loyaltyKudosCalculator.CreateLoyaltyKudosLog(twoYearEmployee, loyaltyKudosType, organizationId,
                kudosYearlyMultipliers, yearOfEmployment);

            // Assert
            Assert.AreEqual(null, loyaltyKudosLog);
            Assert.AreEqual(0, twoYearEmployee.RemainingKudos);
            Assert.AreEqual(10, twoYearEmployee.TotalKudos);
        }

        [Test]
        public void Should_Not_Award_Employee_With_Loyalty_Kudos_Who_Is_Working_0_Years()
        {
            // Setup
            const int organizationId = 2;
            const int yearOfEmployment = 0;
            var kudosYearlyMultipliers = new[] { 1, 2, 3, 4 };
            var oneYearEmployee = new ApplicationUser
            {
                Id = "user",
                OrganizationId = 2,
                RemainingKudos = 0,
                TotalKudos = 10
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10
            };

            // Act
            void Action() => _loyaltyKudosCalculator.CreateLoyaltyKudosLog(oneYearEmployee, loyaltyKudosType, organizationId, kudosYearlyMultipliers, yearOfEmployment);

            // Assert
            var e = Assert.Throws<ArgumentException>(Action);
            Assert.AreEqual("yearOfEmployment", e.ParamName);
        }

        [Test]
        public void Should_Not_Award_If_Employee_Is_Null()
        {
            // Setup
            const int organizationId = 2;
            const int yearOfEmployment = 1;
            var kudosYearlyMultipliers = new[] { 1, 2, 3, 4 };
            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10
            };

            // Act
            void Action() => _loyaltyKudosCalculator.CreateLoyaltyKudosLog(null, loyaltyKudosType, organizationId, kudosYearlyMultipliers, yearOfEmployment);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Action);
            Assert.AreEqual("recipient", e.ParamName);
        }

        [Test]
        public void Should_Not_Award_If_Kudos_Type_Is_Null()
        {
            // Setup
            const int organizationId = 2;
            const int yearOfEmployment = 1;
            var kudosYearlyMultipliers = new[] { 1, 2, 3, 4 };
            var employee = new ApplicationUser
            {
                Id = "user",
                OrganizationId = 2,
                RemainingKudos = 0,
                TotalKudos = 10
            };

            // Act
            void Action() => _loyaltyKudosCalculator.CreateLoyaltyKudosLog(employee, null, organizationId, kudosYearlyMultipliers, yearOfEmployment);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Action);
            Assert.AreEqual("loyaltyKudosType", e.ParamName);
        }

        [Test]
        public void Should_Not_Award_If_Kudos_Yearly_Multipliers_Is_Null()
        {
            // Setup
            const int organizationId = 2;
            const int yearOfEmployment = 3;
            int[] kudosYearlyMultipliers = null;
            var employee = new ApplicationUser
            {
                Id = "user",
                OrganizationId = 2,
                RemainingKudos = 0,
                TotalKudos = 10
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10,
                Name = "Loyalty"
            };

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            void Action() => _loyaltyKudosCalculator.CreateLoyaltyKudosLog(employee, loyaltyKudosType, organizationId, kudosYearlyMultipliers, yearOfEmployment);

            // Assert
            var e = Assert.Throws<ArgumentException>(Action);
            Assert.AreEqual("kudosYearlyMultipliers", e.ParamName);
        }

        [Test]
        public void Should_Not_Award_If_Kudos_Yearly_Multipliers_Is_Empty_Array()
        {
            // Setup
            const int organizationId = 2;
            const int yearOfEmployment = 3;
            var kudosYearlyMultipliers = new int[] { };
            var employee = new ApplicationUser
            {
                Id = "user",
                OrganizationId = 2,
                RemainingKudos = 0,
                TotalKudos = 10
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10,
                Name = "Loyalty"
            };

            // Act
            void Action() => _loyaltyKudosCalculator.CreateLoyaltyKudosLog(employee, loyaltyKudosType, organizationId, kudosYearlyMultipliers, yearOfEmployment);

            // Assert
            var e = Assert.Throws<ArgumentException>(Action);
            Assert.AreEqual("kudosYearlyMultipliers", e.ParamName);
        }

        [Test]
        public void Should_Not_Award_If_Organization_Is_Invalid()
        {
            // Setup
            const int yearOfEmployment = 1;
            var kudosYearlyMultipliers = new[] { 1, 2, 3, 4 };

            var employee = new ApplicationUser
            {
                Id = "user",
                OrganizationId = 2,
                RemainingKudos = 0,
                TotalKudos = 10
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10
            };

            // Act
            void Action() => _loyaltyKudosCalculator.CreateLoyaltyKudosLog(employee, loyaltyKudosType, 0, kudosYearlyMultipliers, yearOfEmployment);

            // Assert
            var e = Assert.Throws<ArgumentException>(Action);
            Assert.AreEqual("organizationId", e.ParamName);
        }

        [Test]
        public void Should_Calculate_Which_Year_Employee_Needs_To_Be_Awarded_With_Loyalty()
        {
            // Setup
            var yearsEmployed = 2;
            var loyaltyAwardsAlreadyReceived = 1;

            // Act
            var yearsToAwardFor = _loyaltyKudosCalculator.CalculateYearsToAwardFor(yearsEmployed, loyaltyAwardsAlreadyReceived);

            // Assert
            Assert.AreEqual(2, yearsToAwardFor.First());
        }

        [Test]
        public void Should_Not_Award_If_Years_To_Award_Is_Negative()
        {
            // Setup
            var yearsEmployed = 2;
            var loyaltyAwardsAlreadyReceived = 3;

            // Act
            var yearsToAwardFor = _loyaltyKudosCalculator.CalculateYearsToAwardFor(yearsEmployed, loyaltyAwardsAlreadyReceived);

            // Assert
            Assert.AreEqual(0, yearsToAwardFor.Count());
        }

        [Test]
        public void Should_Calculate_Which_Year_Employee_Needs_To_Be_Awarded_With_Loyalty_2()
        {
            // Setup
            const int yearsEmployed = 4;
            const int loyaltyAwardsAlreadyReceived = 2;

            // Act
            var yearsToAwardFor = _loyaltyKudosCalculator.CalculateYearsToAwardFor(yearsEmployed, loyaltyAwardsAlreadyReceived).ToList();

            // Assert
            Assert.AreEqual(3, yearsToAwardFor.First());
            Assert.AreEqual(4, yearsToAwardFor.Last());
        }

        [Test]
        public void Should_Calculate_Which_Year_Employee_Needs_To_Be_Awarded_With_Loyalty_3()
        {
            // Setup
            const int yearsEmployed = 0;
            const int loyaltyAwardsAlreadyReceived = 0;

            // Act
            var yearsToAwardFor = _loyaltyKudosCalculator.CalculateYearsToAwardFor(yearsEmployed, loyaltyAwardsAlreadyReceived);

            // Assert
            Assert.AreEqual(0, yearsToAwardFor.Count());
        }

        [Test]
        public void Should_Award_Employee_With_Loyalty_Kudos_OnlyAfter3Years()
        {
            var user1 = new ApplicationUser { Id = "user1", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddYears(-3).AddDays(-10) };
            var user2 = new ApplicationUser { Id = "user2", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddYears(-3).AddDays(-10) };
            var user3 = new ApplicationUser { Id = "user3", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddDays(-100) };

            var employees = new List<ApplicationUser>
            {
                user1, user2, user3
            };

            var organizations = new List<Organization>
            {
                new Organization { Id = 2, ShortName = "tenant2", KudosYearlyMultipliers = "0;0;4" }
            };

            var kudosLogs = new List<KudosLog>
            {
                new KudosLog { Id = 1, EmployeeId = "user1", Created = DateTime.UtcNow.AddYears(-2), KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved, Points = 20, Employee = user1 },
                new KudosLog { Id = 1, EmployeeId = "user1", Created = DateTime.UtcNow.AddYears(-1), KudosTypeName = "Other", OrganizationId = 2, Status = KudosStatus.Approved, Points = 20, Employee = user1 }
            };

            var loyaltyKudosType = new KudosType { Id = 1, Value = 1, Name = "Loyalty" };

            _usersDbSet.SetDbSetDataForAsync(employees.AsQueryable());
            _kudosLogsDbSet.SetDbSetDataForAsync(kudosLogs.AsQueryable());
            _kudosTypeDbSet.SetDbSetDataForAsync(new[] { loyaltyKudosType }.AsQueryable());
            _organizationsDbSet.SetDbSetDataForAsync(organizations.AsQueryable());

            _loyaltyKudosService.AwardEmployeesWithKudos("tenant2");

            // Let's fake that we execute bgr job the second day.
            kudosLogs.Add(new KudosLog { Id = 2, EmployeeId = "user1", Created = DateTime.UtcNow.AddMinutes(-5), KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved, Points = 40, Employee = user1 });
            kudosLogs.Add(new KudosLog { Id = 2, EmployeeId = "user2", Created = DateTime.UtcNow.AddMinutes(-5), KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved, Points = 40, Employee = user2 });
            _loyaltyKudosService.AwardEmployeesWithKudos("tenant2");

            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(x => x.EmployeeId == "user1"));
            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(x => x.EmployeeId == "user2"));
            _kudosLogsDbSet.Received(2).Add(Arg.Any<KudosLog>());
        }

        [Test]
        public void Should_Award_Employee_With_Loyalty_Kudos_OnlyAfter2And3Years()
        {
            var user1 = new ApplicationUser { Id = "user1", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddYears(-3).AddDays(-10) };
            var user2 = new ApplicationUser { Id = "user2", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddYears(-3).AddDays(-10) };
            var user3 = new ApplicationUser { Id = "user3", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddDays(-100) };

            var employees = new List<ApplicationUser>
            {
                user1, user2, user3
            };

            var organizations = new List<Organization>
            {
                new Organization { Id = 2, ShortName = "tenant2", KudosYearlyMultipliers = "0;2;4" }
            };

            var kudosLogs = new List<KudosLog>
            {
                new KudosLog { Id = 1, EmployeeId = "user1", Created = DateTime.UtcNow.AddYears(-2), KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved, Points = 20, Employee = user1 },
                new KudosLog { Id = 1, EmployeeId = "user1", Created = DateTime.UtcNow.AddYears(-1), KudosTypeName = "Other", OrganizationId = 2, Status = KudosStatus.Approved, Points = 20, Employee = user1 }
            };

            var loyaltyKudosType = new KudosType { Id = 1, Value = 1, Name = "Loyalty" };

            _usersDbSet.SetDbSetDataForAsync(employees.AsQueryable());
            _kudosLogsDbSet.SetDbSetDataForAsync(kudosLogs.AsQueryable());
            _kudosTypeDbSet.SetDbSetDataForAsync(new[] { loyaltyKudosType }.AsQueryable());
            _organizationsDbSet.SetDbSetDataForAsync(organizations.AsQueryable());

            _loyaltyKudosService.AwardEmployeesWithKudos("tenant2");

            // Let's fake that we execute bgr job the second day.
            kudosLogs.Add(new KudosLog { Id = 2, EmployeeId = "user1", Created = DateTime.UtcNow.AddMinutes(-5), KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved, Points = 40, Employee = user1 });
            kudosLogs.Add(new KudosLog { Id = 2, EmployeeId = "user2", Created = DateTime.UtcNow.AddMinutes(-5), KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved, Points = 40, Employee = user2 });
            _loyaltyKudosService.AwardEmployeesWithKudos("tenant2");

            _kudosLogsDbSet.Received(2).Add(Arg.Is<KudosLog>(x => x.EmployeeId == "user1"));
            _kudosLogsDbSet.Received(3).Add(Arg.Is<KudosLog>(x => x.EmployeeId == "user2"));
            _kudosLogsDbSet.Received(5).Add(Arg.Any<KudosLog>());
        }

        [Test]
        public void Should_Map_Retrieved_Users_KudosLogs_Organization_And_KudosType_Correctly()
        {
            var user1 = new ApplicationUser { Id = "user1", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddYears(-2).AddDays(-2) };
            var user2 = new ApplicationUser { Id = "user2", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddYears(-2).AddDays(-2) };
            var user3 = new ApplicationUser { Id = "user3", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddDays(-100) };
            var user4 = new ApplicationUser { Id = "user4", OrganizationId = 1, EmploymentDate = null };

            var employees = new List<ApplicationUser>
            {
                user1, user2, user3, user4
            };

            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "tenant1", KudosYearlyMultipliers = "1;2;3;4" },
                new Organization { Id = 2, ShortName = "tenant2", KudosYearlyMultipliers = "1;2;3;4" }
            };

            var kudosLogs = new List<KudosLog>
            {
                new KudosLog { Id = 1, EmployeeId = "user2", KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved, Employee = user2 },
                new KudosLog { Id = 2, EmployeeId = "user2", KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved, Employee = user2 },
                new KudosLog { Id = 3, EmployeeId = "user1", KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved, Employee = user1 },
                new KudosLog { Id = 4, EmployeeId = "user4", KudosTypeName = "Loyalty", OrganizationId = 1, Status = KudosStatus.Approved, Employee = user4 }
            };

            var loyaltyKudosType = new KudosType { Id = 1, Value = 1, Name = "Loyalty" };

            _usersDbSet.SetDbSetDataForAsync(employees.AsQueryable());
            _kudosLogsDbSet.SetDbSetDataForAsync(kudosLogs.AsQueryable());
            _kudosTypeDbSet.SetDbSetDataForAsync(new[] { loyaltyKudosType }.AsQueryable());
            _organizationsDbSet.SetDbSetDataForAsync(organizations.AsQueryable());

            _loyaltyKudosService.AwardEmployeesWithKudos("tenant2");

            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(x => x.EmployeeId == "user1"));
        }
    }
}