using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Shrooms.DataLayer.DAL;
using Shrooms.Domain.Services.WebHookCallbacks.LoyaltyKudos;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Infrastructure.Logger;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.Email.Kudos;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.UnitTests.DomainService
{
    class LoyaltyKudosTests
    {
        private ILoyaltyKudosService _loyaltyKudosService;

        private IDbSet<KudosLog> _kudosLogsDbSet;
        private IDbSet<KudosType> _kudosTypeDbSet;
        private IDbSet<ApplicationUser> _usersDbSet;
        private IDbSet<Organization> _organizationsDbSet;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _kudosLogsDbSet = uow.MockDbSet<KudosLog>();
            _kudosTypeDbSet = uow.MockDbSet<KudosType>();
            _usersDbSet = uow.MockDbSet<ApplicationUser>();
            _organizationsDbSet = uow.MockDbSet<Organization>();

            var loggerMock = Substitute.For<ILogger>();
            var asyncRunner = Substitute.For<IAsyncRunner>();
            _loyaltyKudosService = new LoyaltyKudosService(uow, loggerMock, asyncRunner);
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
                TotalKudos = 10
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10,
                Name = "Loyalty"
            };

            // Act
            var loyaltyKudosLog = LoyaltyKudos.CreateLoyaltyKudosLog(oneYearEmployee, loyaltyKudosType, organizationId,
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
                TotalKudos = 10
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10,
                Name = "Loyalty"
            };

            // Act
            var loyaltyKudosLog = LoyaltyKudos.CreateLoyaltyKudosLog(twoYearEmployee, loyaltyKudosType, organizationId,
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
                TotalKudos = 10
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10,
                Name = "Loyalty"
            };

            // Act
            var loyaltyKudosLog = LoyaltyKudos.CreateLoyaltyKudosLog(threeYearEmployee, loyaltyKudosType, organizationId,
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
                TotalKudos = 10
            };

            var loyaltyKudosType = new KudosType
            {
                Id = 1,
                Value = 10,
                Name = "Loyalty"
            };

            // Act
            var loyaltyKudosLog = LoyaltyKudos.CreateLoyaltyKudosLog(threeYearEmployee, loyaltyKudosType, organizationId,
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
            var loyaltyKudosLog = LoyaltyKudos.CreateLoyaltyKudosLog(twoYearEmployee, loyaltyKudosType, organizationId,
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

            // Assert, Act
            var e = Assert.Throws<ArgumentException>(() => LoyaltyKudos.CreateLoyaltyKudosLog(oneYearEmployee, loyaltyKudosType, organizationId, kudosYearlyMultipliers, yearOfEmployment));
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

            // Assert, Act
            var e = Assert.Throws<ArgumentNullException>(() => LoyaltyKudos.CreateLoyaltyKudosLog(null, loyaltyKudosType, organizationId, kudosYearlyMultipliers, yearOfEmployment));
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

            // Assert, Act
            var e = Assert.Throws<ArgumentNullException>(() => LoyaltyKudos.CreateLoyaltyKudosLog(employee, null, organizationId, kudosYearlyMultipliers, yearOfEmployment));
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

            // Assert, Act
            var e = Assert.Throws<ArgumentException>(() => LoyaltyKudos.CreateLoyaltyKudosLog(employee, loyaltyKudosType, organizationId, kudosYearlyMultipliers, yearOfEmployment));
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

            // Assert, Act
            var e = Assert.Throws<ArgumentException>(() => LoyaltyKudos.CreateLoyaltyKudosLog(employee, loyaltyKudosType, organizationId, kudosYearlyMultipliers, yearOfEmployment));
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

            // Assert, Act
            var e = Assert.Throws<ArgumentException>(() => LoyaltyKudos.CreateLoyaltyKudosLog(employee, loyaltyKudosType, 0, kudosYearlyMultipliers, yearOfEmployment));
            Assert.AreEqual("organizationId", e.ParamName);
        }

        [Test]
        public void Should_Calculate_Which_Year_Employee_Needs_To_Be_Awarded_With_Loyalty()
        {
            // Setup
            var yearsEmployed = 2;
            var loyaltyAwardsAlreadyReceived = 1;

            // Act
            var yearsToAwardFor = LoyaltyKudos.CalculateYearsToAwardFor(yearsEmployed, loyaltyAwardsAlreadyReceived);

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
            var yearsToAwardFor = LoyaltyKudos.CalculateYearsToAwardFor(yearsEmployed, loyaltyAwardsAlreadyReceived);

            // Assert
            Assert.AreEqual(0, yearsToAwardFor.Count());
        }

        [Test]
        public void Should_Calculate_Which_Year_Employee_Needs_To_Be_Awarded_With_Loyalty_2()
        {
            // Setup
            var yearsEmployed = 4;
            var loyaltyAwardsAlreadyReceived = 2;

            // Act
            var yearsToAwardFor = LoyaltyKudos.CalculateYearsToAwardFor(yearsEmployed, loyaltyAwardsAlreadyReceived);

            // Assert
            Assert.AreEqual(3, yearsToAwardFor.First());
            Assert.AreEqual(4, yearsToAwardFor.Last());
        }

        [Test]
        public void Should_Calculate_Which_Year_Employee_Needs_To_Be_Awarded_With_Loyalty_3()
        {
            // Setup
            var yearsEmployed = 0;
            var loyaltyAwardsAlreadyReceived = 0;

            // Act
            var yearsToAwardFor = LoyaltyKudos.CalculateYearsToAwardFor(yearsEmployed, loyaltyAwardsAlreadyReceived);

            // Assert
            Assert.AreEqual(0, yearsToAwardFor.Count());
        }

        [Test]
        public void Should_Award_Employee_With_Loyalty_Kudos_OnlyAfter3Years()
        {
            var employees = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddYears(-3).AddDays(-10) },
                new ApplicationUser { Id = "user2", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddYears(-3).AddDays(-10) },
                new ApplicationUser { Id = "user3", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddDays(-100) }
            };

            var organizations = new List<Organization>
            {
                new Organization { Id = 2, ShortName = "tenant2", KudosYearlyMultipliers = "0;0;4" }
            };

            var kudosLogs = new List<KudosLog>
            {
                new KudosLog { Id = 1, EmployeeId = "user1", KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved, Points = 20 }
            };

            var loyaltyKudosType = new KudosType { Id = 1, Value = 1, Name = "Loyalty" };

            _usersDbSet.SetDbSetData(employees.AsQueryable());
            _kudosLogsDbSet.SetDbSetData(kudosLogs.AsQueryable());
            _kudosTypeDbSet.SetDbSetData(new[] { loyaltyKudosType }.AsQueryable());
            _organizationsDbSet.SetDbSetData(organizations.AsQueryable());
            
            _loyaltyKudosService.AwardEmployeesWithKudos("tenant2");
            // Let's fake that we execute bgr job the second day.
            kudosLogs.Add(new KudosLog { Id = 2, EmployeeId = "user1", KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved, Points = 40 });
            kudosLogs.Add(new KudosLog { Id = 2, EmployeeId = "user2", KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved, Points = 40 });
            _loyaltyKudosService.AwardEmployeesWithKudos("tenant2");

            _kudosLogsDbSet.Received(2).Add(Arg.Any<KudosLog>());
            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(x => x.EmployeeId == "user1"));
            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(x => x.EmployeeId == "user2"));
        }

        [Test]
        public void Should_Map_Retrieved_Users_KudosLogs_Organization_And_KudosType_Correctly()
        {
            var employees = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "user1", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddYears(-2).AddDays(-2) },
                new ApplicationUser { Id = "user2", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddYears(-2).AddDays(-2) },
                new ApplicationUser { Id = "user3", OrganizationId = 2, EmploymentDate = DateTime.UtcNow.AddDays(-100) },
                new ApplicationUser { Id = "user4", OrganizationId = 1, EmploymentDate = null }
            };

            var organizations = new List<Organization>
            {
                new Organization { Id = 1, ShortName = "tenant1", KudosYearlyMultipliers = "1;2;3;4" },
                new Organization { Id = 2, ShortName = "tenant2", KudosYearlyMultipliers = "1;2;3;4" }
            };

            var kudosLogs = new List<KudosLog>
            {
                new KudosLog { Id = 1, EmployeeId = "user2", KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved },
                new KudosLog { Id = 2, EmployeeId = "user2", KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved },
                new KudosLog { Id = 3, EmployeeId = "user1", KudosTypeName = "Loyalty", OrganizationId = 2, Status = KudosStatus.Approved },
                new KudosLog { Id = 4, EmployeeId = "user4", KudosTypeName = "Loyalty", OrganizationId = 1, Status = KudosStatus.Approved }
            };

            var loyaltyKudosType = new KudosType { Id = 1, Value = 1, Name = "Loyalty" };

            _usersDbSet.SetDbSetData(employees.AsQueryable());
            _kudosLogsDbSet.SetDbSetData(kudosLogs.AsQueryable());
            _kudosTypeDbSet.SetDbSetData(new[] { loyaltyKudosType }.AsQueryable());
            _organizationsDbSet.SetDbSetData(organizations.AsQueryable());

            _loyaltyKudosService.AwardEmployeesWithKudos("tenant2");

            _kudosLogsDbSet.Received(1).Add(Arg.Is<KudosLog>(x => x.EmployeeId == "user1"));
        }
    }
}