using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Web;
using DomainServiceValidators.Validators.Kudos;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Constants.Authorization;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.DomainExceptions.Exceptions.Kudos;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.UnitTests.Extensions;

namespace Shrooms.API.Tests.DomainService
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "Acceptable for tests")]
    public class KudosServiceTests
    {
        private IKudosService _kudosService;
        private IDbSet<KudosLog> _kudosLogsDbSet;
        private IDbSet<ApplicationUser> _usersDbSet;
        private IDbSet<KudosType> _kudosTypesDbSet;
        private IDbSet<Organization> _organizationDbSet;
        private IUnitOfWork2 _uow;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _kudosLogsDbSet = Substitute.For<IDbSet<KudosLog>>();
            _kudosLogsDbSet.SetDbSetData(MockKudosLogs());
            _usersDbSet = Substitute.For<IDbSet<ApplicationUser>>();
            _usersDbSet.SetDbSetData(MockUsers());
            _kudosTypesDbSet = Substitute.For<IDbSet<KudosType>>();
            _kudosTypesDbSet.SetDbSetData(MockKudosTypes());
            _organizationDbSet = Substitute.For<IDbSet<Organization>>();
            _organizationDbSet.SetDbSetData(MockOrganization());

            MockFindMethod();

            _uow.GetDbSet<KudosLog>().Returns(_kudosLogsDbSet);
            _uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);
            _uow.GetDbSet<KudosType>().Returns(_kudosTypesDbSet);
            _uow.GetDbSet<Organization>().Returns(_organizationDbSet);

            var uow2 = Substitute.For<IUnitOfWork>();

            var kudosServiceValidation = MockServiceValidator();
            var permissionService = MockPermissionService();
            var roleService = Substitute.For<IRoleService>();
            var asyncRunner = Substitute.For<IAsyncRunner>();
            MockRoleService(roleService);

            _kudosService = new KudosService(_uow, uow2, roleService, permissionService, kudosServiceValidation, asyncRunner);
        }

        #region GetKudosLogs

        [Test]
        public void Should_Return_All_Kudos_Logs()
        {
            var filter = new KudosLogsFilterDTO
            {
                OrganizationId = 2,
                Page = 1,
                Status = ConstBusinessLayer.KudosStatusAllFilter,
                UserId = "testUserId",
                SortBy = "Created",
                SortOrder = "desc"
            };
            var result = _kudosService.GetKudosLogs(filter);
            Assert.AreEqual(4, result.TotalKudosCount);
        }

        [Test]
        public void Should_Return_Kudos_Logs_Filtered_By_Status_Approved()
        {
            var filter = new KudosLogsFilterDTO
            {
                OrganizationId = 2,
                Page = 1,
                Status = "Approved",
                UserId = "testUserId",
                SortBy = "Created",
                SortOrder = "desc"
            };
            var result = _kudosService.GetKudosLogs(filter);
            Assert.AreEqual(2, result.TotalKudosCount);
        }

        [Test]
        public void Should_Return_Return_All_Kudos_Logs_With_Organization_Filter()
        {
            MockKudosLogsForOrganizationTest();
            var filter = new KudosLogsFilterDTO
            {
                OrganizationId = 2,
                Page = 1,
                Status = ConstBusinessLayer.KudosStatusAllFilter,
                SortBy = "Created",
                SortOrder = "desc"
            };
            var result = _kudosService.GetKudosLogs(filter);
            Assert.AreEqual(1, result.KudosLogs.Count());
            Assert.AreEqual(1, result.KudosLogs.First().Id);
        }

        [Test]
        public void Should_Return_Specific_User_Kudos_Logs()
        {
            var result = _kudosService.GetUserKudosLogs("testUserId", 1, 2);
            Assert.AreEqual(3, result.TotalKudosCount);
        }

        [Test]
        public void Should_Return_Specific_User_Kudos_Logs_With_Organization_Filter()
        {
            MockKudosLogsForOrganizationTest();
            var result = _kudosService.GetUserKudosLogs("UserId", 1, 1);
            Assert.AreEqual(1, result.KudosLogs.Count());
            Assert.AreEqual(2, result.KudosLogs.First().Id);
        }

        [Test]
        public void Should_Return_Last_Five_Approved_Kudos_Logs()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2
            };
            var result = _kudosService.GetLastKudosLogsForWall(userAndOrg);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void Should_Return_Kudos_Logs_For_Wall_With_Organization_Filter()
        {
            MockKudosLogsForOrganizationTest();
            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 1,
                UserId = "UserId"
            };
            var result = _kudosService.GetLastKudosLogsForWall(userAndOrg);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Comment2", result.First().Comment);
        }

        [Test]
        public void Should_Return_Kudos_Logs_For_Pie_Chart_With_Organization_Filter()
        {
            MockKudosLogsForPieChart();
            var result = _kudosService.GetKudosPieChartData(1, "UserId");
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(3, result.First().Value);
        }

        [Test]
        public void Should_Return_Kudos_Logs_For_Pie_Chart_With_Organization_Filter_2()
        {
            MockKudosLogsForPieChart();
            var result = _kudosService.GetKudosPieChartData(2, "UserId");
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(2, result.First().Value);
            Assert.AreEqual(10, result.ToArray()[1].Value);
            Assert.AreEqual("Type2", result.ToArray()[1].Name);
        }

        [Test]
        public void Should_Return_Approved_Kudos_Logs_With_Organization_Filter()
        {
            var test = _usersDbSet.Find("CreatedUserId");

            MockKudosLogsForApprovedList();
            var result = _kudosService.GetApprovedKudosList("UserId", 1);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Comment2", result.First().Comments);
            Assert.AreEqual("CreatedUserId", result.First().Sender.Id);
        }

        [Test]
        public void Should_Return_Approved_Kudos_Logs_With_Organization_Filter_2()
        {
            MockKudosLogsForApprovedList();
            var result = _kudosService.GetApprovedKudosList("UserId", 2);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Comment1", result.First().Comments);
            Assert.AreEqual("CreatedUserId", result.First().Sender.Id);
            Assert.AreEqual("Comment3", result.ToArray()[1].Comments);
        }
        #endregion

        #region GetKudosTypes

        [Test]
        public void Should_Return_All_Kudos_Types()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                UserId = "testUserId2"
            };

            var types = _kudosService.GetKudosTypes(userAndOrg);
            Assert.AreEqual(4, types.Count());
        }

        [Test]
        public void Should_Return_Basic_Kudos_Types()
        {
            var userAndOrg = new UserAndOrganizationDTO
            {
                UserId = "testUserId"
            };

            var types = _kudosService.GetKudosTypes(userAndOrg);
            Assert.AreEqual(3, types.Count());
        }
        #endregion

        #region UpdateKudosLogs

        [Test]
        public void Should_Throw_When_Approving_Other_Organization_Log()
        {
            MockKudosLogsForUpdate();

            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 1,
                UserId = "UserId"
            };

            Assert.Throws<InvalidOperationException>(() => _kudosService.ApproveKudos(1, userAndOrg));
        }

        [Test]
        public void Should_Return_When_Kudos_Log_Was_Not_Approved()
        {
            MockKudosLogsForUpdate();

            var userAndOrg = new UserAndOrganizationDTO
            {
                OrganizationId = 2,
                UserId = "UserId2"
            };

            _kudosService.ApproveKudos(1, userAndOrg);
            Assert.AreEqual(_kudosLogsDbSet.First().Points, _kudosLogsDbSet.First().Employee.RemainingKudos);
            Assert.AreEqual(KudosStatus.Approved, _kudosLogsDbSet.First().Status);
        }

        [Test]
        public void Should_Return_If_Rejection_Doesnt_Update_Status()
        {
            var kudosRejectDTO = new KudosRejectDTO()
            {
                OrganizationId = 2,
                UserId = "testUserId",
                id = 1,
                kudosRejectionMessage = "testMessage"
            };

            _kudosService.RejectKudos(kudosRejectDTO);

            var log = _kudosLogsDbSet.First(x => x.Id == 1);
            Assert.AreEqual(KudosStatus.Rejected, log.Status);
            Assert.AreEqual("testMessage", log.RejectionMessage);
        }

        [Test]
        public void Should_Throw_When_Rejecting_Other_Organization_Log()
        {
            MockKudosLogsForUpdate();

            var kudosRejectDTO = new KudosRejectDTO()
            {
                OrganizationId = 1,
                UserId = "testUserId",
                id = 1,
                kudosRejectionMessage = "testMessage"
            };

            Assert.Throws<InvalidOperationException>(() => _kudosService.RejectKudos(kudosRejectDTO));
        }

        [Test]
        public void Should_Update_User_Profile_Kudos_Depending_On_Logs()
        {
            var user = new ApplicationUser()
            {
                Id = "Id",
                EmploymentDate = DateTime.UtcNow.AddDays(-10)
            };

            var userOrg = new UserAndOrganizationDTO()
            {
                OrganizationId = 1,
                UserId = "userId"
            };

            MockKudosLogsForProfileUpdate();

            _kudosService.UpdateProfileKudos(user, userOrg);
            Assert.AreEqual(11, user.TotalKudos);
            Assert.AreEqual(9, user.RemainingKudos);
            Assert.AreEqual(2, user.SpentKudos);
        }

        #endregion

        #region AddKudosLogs

        //Checks if permission validation for minus kudos operation works properly.
        [Test]
        public void Should_Return_If_User_Has_No_Permission()
        {
            var kudosLog = new AddKudosLogDTO
            {
                OrganizationId = 2,
                PointsTypeId = 1,
                UserId = "testUserId",
                ReceivingUserIds = new List<string>() { "testUserId" },
                MultiplyBy = 2,
                Comment = "Comment"
            };

            Assert.Throws<KudosException>(() => _kudosService.AddKudosLog(kudosLog));
        }

        //Checks if kudos logs has been saved for kudos minus operation.
        [Test]
        public void Should_Return_If_Kudos_Logs_Has_Not_Been_Saved_1()
        {
            var kudosLog = new AddKudosLogDTO
            {
                OrganizationId = 2,
                PointsTypeId = 1,
                UserId = "testUserId2",
                ReceivingUserIds = new List<string>() { "testUserId3", "testUserId4" },
                MultiplyBy = 2,
                Comment = "Comment"
            };

            _kudosService.AddKudosLog(kudosLog);
            _kudosLogsDbSet.Received(2).Add(Arg.Any<KudosLog>());
            _uow.Received(1).SaveChanges(false);
        }

        [Test]
        public void AddKudosLog_OverridenPointsAmountPassed_SaveTriggeredWithExplicitAmount()
        {
            // Arrange
            var explicitAmount = 1234564;
            var kudosLog = new AddKudosLogDTO
            {
                OrganizationId = 2,
                PointsTypeId = 2,
                UserId = "testUserId2",
                ReceivingUserIds = new List<string>() { "testUserId3", "testUserId4" },
                MultiplyBy = 2,
                Comment = "Comment"
            };

            // Act
            _kudosService.AddKudosLog(kudosLog, explicitAmount);

            // Assert
            _kudosLogsDbSet.Received(4).Add(Arg.Is<KudosLog>(l => l.Points == explicitAmount));
        }

        //Checks if kudos logs has been saved for kudos send operation.
        [Test]
        public void Should_Return_If_Kudos_Logs_Has_Not_Been_Saved_2()
        {
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://shrooms", ""),
                new HttpResponse(new StringWriter()));

            var kudosLog = new AddKudosLogDTO
            {
                OrganizationId = 2,
                PointsTypeId = 2,
                UserId = "testUserId2",
                ReceivingUserIds = new List<string>() { "testUserId3", "testUserId4" },
                MultiplyBy = 2,
                Comment = "Comment"
            };

            _kudosService.AddKudosLog(kudosLog);
            _kudosLogsDbSet.Received(4).Add(Arg.Any<KudosLog>());
            _uow.Received(1).SaveChanges(false);
        }

        //Checks if available kudos validation for send kudos operation works properly.
        [Test]
        public void Should_Return_If_User_Has_No_Available_Kudos()
        {
            _kudosTypesDbSet.SetDbSetData(MockKudosTypes());
            _kudosLogsDbSet.SetDbSetData(MockKudosLogs());

            var kudosLog = new AddKudosLogDTO
            {
                OrganizationId = 2,
                PointsTypeId = 2,
                UserId = "testUserId2",
                ReceivingUserIds = new List<string>() { "testUserId3", "testUserId4", "testUserId" },
                MultiplyBy = 2,
                Comment = "Comment"
            };

            Assert.Throws<KudosException>(() => _kudosService.AddKudosLog(kudosLog));
        }

        //Checks if validation for send kudos to same user workd properly.
        [Test]
        public void Should_Return_When_User_Sends_Kudos_To_Himself()
        {
            var kudosLog = new AddKudosLogDTO
            {
                OrganizationId = 2,
                PointsTypeId = 2,
                UserId = "testUserId2",
                ReceivingUserIds = new List<string>() { "testUserId3", "testUserId2" },
                MultiplyBy = 1,
                Comment = "Comment"
            };

            Assert.Throws<KudosException>(() => _kudosService.AddKudosLog(kudosLog));
        }

        //User can send limited amount of kudos per month, so this test
        //checks if validation for monthly sending kudos works properly.
        [Test]
        public void Should_Return_If_User_Has_No_Available_Monthly_Kudos()
        {
            var kudosLog = new AddKudosLogDTO
            {
                OrganizationId = 2,
                PointsTypeId = 2,
                UserId = "testUserId5",
                ReceivingUserIds = new List<string>() { "testUserId3", "testUserId4", "testUserId" },
                MultiplyBy = 2,
                Comment = "Comment"
            };

            Assert.Throws<KudosException>(() => _kudosService.AddKudosLog(kudosLog));
        }

        //Checks if kudos logs has been saved for kudos add operation.
        [Test]
        public void Should_Return_If_Kudos_Logs_Has_Not_Been_Saved_3()
        {
            var kudosLog = new AddKudosLogDTO
            {
                OrganizationId = 2,
                PointsTypeId = 3,
                UserId = "testUserId2",
                ReceivingUserIds = new List<string>() { "testUserId3", "testUserId4" },
                MultiplyBy = 2,
                Comment = "Comment"
            };

            _kudosService.AddKudosLog(kudosLog);
            _kudosLogsDbSet.Received(2).Add(Arg.Any<KudosLog>());
            _uow.Received(1).SaveChanges(false);
        }
        #endregion

        #region GetKudosStats

        [Test]
        public void Should_Return_Correctly_Summed_Kudos()
        {
            MockKudosLogsForStats();
            var actual = _kudosService.GetKudosStats(3, 10, 2).ToList();
            Assert.AreEqual("User 1", actual[0].Name);
            Assert.AreEqual(274.4, actual[0].KudosAmount);
            Assert.AreEqual("User 3", actual[1].Name);
            Assert.AreEqual(34, actual[1].KudosAmount);
            Assert.AreEqual("User 2", actual[2].Name);
            Assert.AreEqual(20, actual[2].KudosAmount);
            Assert.AreEqual("User 4", actual[3].Name);
            Assert.AreEqual(10, actual[3].KudosAmount);
        }

        #endregion

        #region HasPendingKudos

        [Test]
        public void HasPendingKudos_Should_Return_True()
        {
            MockKudosLogsForStats();
            var actual = _kudosService.HasPendingKudos("User1");
            Assert.AreEqual(true, actual);
        }

        [Test]
        public void HasPendingKudos_Should_Return_False()
        {
            MockKudosLogsForStats();
            var actual = _kudosService.HasPendingKudos("User2");
            Assert.AreEqual(false, actual);
        }

        #endregion

        #region GetNecessaryKudosTypes
        [Test]
        public void Should_Return_All_Necessary_Kudos_Types()
        {
            var expectedCollection = new List<KudosTypeDTO>
            {
                new KudosTypeDTO
                {
                    Id = 1,
                    Name = "Minus",
                    Value = 1,
                    Type = ConstBusinessLayer.KudosTypeEnum.Minus
                },
                new KudosTypeDTO
                {
                    Id = 2,
                    Name = "Send",
                    Value = 1,
                    Type = ConstBusinessLayer.KudosTypeEnum.Send
                },
                new KudosTypeDTO
                {
                    Id = 4,
                    Name = "Other",
                    Value = 3,
                    Type = ConstBusinessLayer.KudosTypeEnum.Other
                }
            };

            var userAndOrg = new UserAndOrganizationDTO
            {
                UserId = "testUserId2"
            };

            var types = _kudosService.GetNecessaryKudosTypes(userAndOrg);
            Assert.AreEqual(3, types.Count());
            CollectionAssert.Contains(types, expectedCollection[0]);
            CollectionAssert.Contains(types, expectedCollection[1]);
            CollectionAssert.Contains(types, expectedCollection[2]);
        }

        [Test]
        public void Should_Return_Necessary_Kudos_Types_Without_Minus()
        {
            var expectedCollection = new List<KudosTypeDTO>
            {
                new KudosTypeDTO
                {
                    Id = 2,
                    Name = "Send",
                    Value = 1,
                    Type = ConstBusinessLayer.KudosTypeEnum.Send
                },
                new KudosTypeDTO
                {
                    Id = 4,
                    Name = "Other",
                    Value = 3,
                    Type = ConstBusinessLayer.KudosTypeEnum.Other
                }
            };

            var userAndOrg = new UserAndOrganizationDTO
            {
                UserId = "testUserId"
            };

            var types = _kudosService.GetNecessaryKudosTypes(userAndOrg);
            Assert.AreEqual(2, types.Count());
            CollectionAssert.Contains(types, expectedCollection[0]);
            CollectionAssert.Contains(types, expectedCollection[1]);
        }
        #endregion

        #region MockData

        private static void MockRoleService(IRoleService roleService)
        {
            roleService.ExcludeUsersWithRole(Roles.NewUser).Returns(x => true);
        }

        private IPermissionService MockPermissionService()
        {
            var permissionService = Substitute.For<IPermissionService>();

            permissionService.UserHasPermission(Arg.Is<UserAndOrganizationDTO>(x => x.UserId == "testUserId"), Arg.Any<string>()).Returns(false);
            permissionService.UserHasPermission(Arg.Is<UserAndOrganizationDTO>(x => x.UserId == "testUserId2"), Arg.Any<string>()).Returns(true);
            return permissionService;
        }

        private IKudosServiceValidator MockServiceValidator()
        {
            var kudosServiceValidation = Substitute.For<IKudosServiceValidator>();
            kudosServiceValidation
                .When(x => x.ValidateKudosMinusPermission(false))
                .Do(x => throw new KudosException(""));

            kudosServiceValidation
                .When(x => x.ValidateUserAvailableKudos(4, 6))
                .Do(x => throw new KudosException(""));

            kudosServiceValidation
                .When(x => x.ValidateUserAvailableKudosToSendPerMonth(2, 0))
                .Do(x => throw new KudosException(""));

            kudosServiceValidation
               .When(x => x.ValidateSendingToSameUserAsReceiving("testUserId2", "testUserId2"))
               .Do(x => throw new KudosException(""));
            return kudosServiceValidation;
        }

        private void MockFindMethod()
        {
            _organizationDbSet.Find(2).Returns(MockOrganization().FirstOrDefault(x => x.Id == 2));

            _kudosTypesDbSet.Find(1).Returns(MockKudosTypes().FirstOrDefault(x => x.Id == 1));
            _kudosTypesDbSet.Find(2).Returns(MockKudosTypes().FirstOrDefault(x => x.Id == 2));
            _kudosTypesDbSet.Find(3).Returns(MockKudosTypes().FirstOrDefault(x => x.Id == 3));

            _usersDbSet.Find("testUserId").Returns(MockUsers().FirstOrDefault(x => x.Id == "testUserId"));
            _usersDbSet.Find("testUserId2").Returns(MockUsers().FirstOrDefault(x => x.Id == "testUserId2"));
            _usersDbSet.Find("testUserId3").Returns(MockUsers().FirstOrDefault(x => x.Id == "testUserId3"));
            _usersDbSet.Find("testUserId4").Returns(MockUsers().FirstOrDefault(x => x.Id == "testUserId4"));
            _usersDbSet.Find("testUserId5").Returns(MockUsers().FirstOrDefault(x => x.Id == "testUserId5"));
            _usersDbSet.Find("CreatedUserId").Returns(MockUsers().FirstOrDefault(x => x.Id == "CreatedUserId"));
        }

        private IQueryable<Organization> MockOrganization()
        {
            return new List<Organization>
            {
                new Organization
                {
                    ShortName = "Visma",
                    Id = 2
                }
            }.AsQueryable();
        }

        private IQueryable<ApplicationUser> MockUsers()
        {
            return new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "testUserId",
                    FirstName = "name",
                    LastName = "surname",
                    OrganizationId = 2,
                    CultureCode = "en-US"
                },
                new ApplicationUser
                {
                    Id = "testUserId2",
                    TotalKudos = 4,
                    SpentKudos = 0,
                    RemainingKudos = 4,
                    OrganizationId = 2,
                    CultureCode = "en-US"
                },
                new ApplicationUser
                {
                    Id = "testUserId3",
                    TotalKudos = 10,
                    SpentKudos = 2,
                    RemainingKudos = 8,
                    OrganizationId = 2
                },
                new ApplicationUser
                {
                    Id = "testUserId4",
                    TotalKudos = 2,
                    SpentKudos = 0,
                    RemainingKudos = 2,
                    OrganizationId = 2
                },
                new ApplicationUser
                {
                    Id = "testUserId5",
                    TotalKudos = 12,
                    SpentKudos = 2,
                    RemainingKudos = 10,
                    OrganizationId = 2
                },
                new ApplicationUser
                {
                    Id = "CreatedUserId",
                    OrganizationId = 2
                }
            }.AsQueryable();
        }

        private IQueryable<KudosType> MockKudosTypes()
        {
            return new List<KudosType>
            {
                new KudosType
                {
                    Id = 1,
                    Name = "Minus",
                    Value = 1,
                    Type = ConstBusinessLayer.KudosTypeEnum.Minus
                },
                new KudosType
                {
                    Id = 2,
                    Name = "Send",
                    Value = 1,
                    Type = ConstBusinessLayer.KudosTypeEnum.Send
                },
                new KudosType
                {
                    Id = 3,
                    Name = "AnythingElse",
                    Value = 2,
                    Type = ConstBusinessLayer.KudosTypeEnum.Ordinary
                },
                new KudosType
                {
                    Id = 4,
                    Name = "Other",
                    Value = 3,
                    Type = ConstBusinessLayer.KudosTypeEnum.Other
                },
            }.AsQueryable();
        }

        private IQueryable<KudosLog> MockKudosLogs()
        {
            var kudosLogs = new List<KudosLog>
            {
                new KudosLog
                {
                    Status = KudosStatus.Pending,
                    Id = 1,
                    EmployeeId = "testUserId",
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name",
                        LastName = "surname"
                    },
                    KudosTypeName = "Type1",
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    OrganizationId = 2,
                    CreatedBy = "testUserId"
                },
                new KudosLog
                {
                    Status = KudosStatus.Pending,
                    Id = 2,
                    EmployeeId = "testUserId",
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name",
                        LastName = "surname"
                    },
                    KudosTypeName = "Type1",
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    OrganizationId = 2,
                    CreatedBy = "testUserId"
                },
                new KudosLog
                {
                    Status = KudosStatus.Approved,
                    Id = 3,
                    EmployeeId = "testUserId",
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name",
                        LastName = "surname"
                    },
                    KudosTypeName = "Type2",
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    OrganizationId = 2,
                    CreatedBy = "testUserId",
                    Points = 0,
                    Comments = "Hello"
                },
                new KudosLog
                {
                    Status = KudosStatus.Approved,
                    Id = 4,
                    EmployeeId = "testUserId3",
                    KudosTypeName = "Send",
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Send,
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name",
                        LastName = "surname"
                    },
                    OrganizationId = 2,
                    CreatedBy = "testUserId5",
                    MultiplyBy = ConstBusinessLayer.KudosAvailableToSendThisMonth - 2,
                    Created = DateTime.UtcNow,
                    Points = ConstBusinessLayer.KudosAvailableToSendThisMonth - 2,
                    Comments = "Hello"
                }
            };
            return kudosLogs.AsQueryable();
        }

        private void MockKudosLogsForOrganizationTest()
        {
            var kudosLogs = new List<KudosLog>
            {
                new KudosLog
                {
                    Id = 1,
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    Employee = new ApplicationUser
                    {
                        Id = "UserId",
                        FirstName = "Name",
                        LastName = "Surname"
                    },
                    EmployeeId = "UserId",
                    OrganizationId = 2,
                    Comments = "Comment1",
                    MultiplyBy = 1,
                    Points = 2,
                    CreatedBy = "CreatedUserId",
                    Status = KudosStatus.Approved,
                    Created = DateTime.UtcNow
                },
                new KudosLog
                {
                    Id = 2,
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    Employee = new ApplicationUser
                    {
                        Id = "UserId",
                        FirstName = "Name",
                        LastName = "Surname"
                    },
                    EmployeeId = "UserId",
                    OrganizationId = 1,
                    Comments = "Comment2",
                    MultiplyBy = 1,
                    Points = 2,
                    CreatedBy = "CreatedUserId",
                    Status = KudosStatus.Approved,
                    Created = DateTime.UtcNow
                }
            };

            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "CreatedUserId"
                }
            };

            _usersDbSet.SetDbSetData(users.AsQueryable());
            _kudosLogsDbSet.SetDbSetData(kudosLogs.AsQueryable());
        }

        private void MockKudosLogsForPieChart()
        {
            var kudosLogs = new List<KudosLog>
            {
                new KudosLog
                {
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    EmployeeId = "UserId",
                    OrganizationId = 2,
                    MultiplyBy = 1,
                    Points = 2,
                    Status = KudosStatus.Approved,
                },
                new KudosLog
                {
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    EmployeeId = "UserId",
                    OrganizationId = 1,
                    Points = 3,
                    Status = KudosStatus.Approved,
                },
                new KudosLog
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    EmployeeId = "UserId",
                    OrganizationId = 2,
                    Points = 10,
                    Status = KudosStatus.Approved,
                }
            };
            _kudosLogsDbSet.SetDbSetData(kudosLogs.AsQueryable());
        }

        private void MockKudosLogsForApprovedList()
        {
            var kudosLogs = new List<KudosLog>
            {
                new KudosLog
                {
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    EmployeeId = "UserId",
                    Comments = "Comment1",
                    OrganizationId = 2,
                    MultiplyBy = 1,
                    Points = 2,
                    Status = KudosStatus.Approved,
                    CreatedBy = "CreatedUserId"
                },
                new KudosLog
                {
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    EmployeeId = "UserId",
                    Comments = "Comment2",
                    OrganizationId = 1,
                    Points = 3,
                    Status = KudosStatus.Approved,
                    CreatedBy = "CreatedUserId"
                },
                new KudosLog
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    EmployeeId = "UserId",
                    Comments = "Comment3",
                    OrganizationId = 2,
                    Points = 10,
                    Status = KudosStatus.Approved,
                    CreatedBy = "CreatedUserId"
                }
            };

            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "CreatedUserId"
                }
            };

            _usersDbSet.SetDbSetData(users.AsQueryable());
            _kudosLogsDbSet.SetDbSetData(kudosLogs.AsQueryable());
        }

        private void MockKudosLogsForUpdate()
        {
            var kudosLogs = new List<KudosLog>
            {
                new KudosLog
                {
                    Id = 1,
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    Employee = new ApplicationUser
                    {
                        Id = "UserId",
                        FirstName = "Name",
                        LastName = "Surname",
                        EmploymentDate = DateTime.UtcNow.AddDays(-1)
                    },
                    EmployeeId = "UserId",
                    OrganizationId = 2,
                    Comments = "Comment1",
                    MultiplyBy = 1,
                    Points = 2,
                    Status = KudosStatus.Pending,
                    Created = DateTime.UtcNow,
                    CreatedBy = "CreatedUserId"
                }
            };

            var organizations = new List<Organization>
            {
                new Organization
                {
                    Id = 2,
                    ShortName = "VismaShortName"
                }
            };

            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                   Id = "CreatedUserId",
                   FirstName = "Name",
                   LastName = "Surname",
                   TotalKudos = 0,
                   RemainingKudos = 0
                }
            };

            _usersDbSet.SetDbSetData(users.AsQueryable());
            _organizationDbSet.SetDbSetData(organizations.AsQueryable());
            _kudosLogsDbSet.SetDbSetData(kudosLogs.AsQueryable());
        }

        private void MockKudosLogsForStats()
        {
            // User1 has 274.4 Kudos
            // User2 has 20
            // USer3 has 33
            var kudosLogs = new List<KudosLog>
            {
                new KudosLog // +2
                {
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    Employee = new ApplicationUser
                    {
                        Id = "User1",
                        FirstName = "User",
                        LastName = "1",
                        EmploymentDate = DateTime.UtcNow
                    },
                    EmployeeId = "User1",
                    OrganizationId = 2,
                    MultiplyBy = 2,
                    Points = 2,
                    Status = KudosStatus.Approved,
                    Created = DateTime.UtcNow
                },
                new KudosLog // +30
                {
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    Employee = new ApplicationUser
                    {
                        Id = "User1",
                        FirstName = "User",
                        LastName = "1",
                        EmploymentDate = DateTime.UtcNow
                    },
                    EmployeeId = "User1",
                    OrganizationId = 2,
                    Points = 30,
                    MultiplyBy = 30,
                    Status = KudosStatus.Approved,
                    Created = DateTime.UtcNow
                },
                new KudosLog // 0 (OrgID = 2)
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    Employee = new ApplicationUser
                    {
                        Id = "User1",
                        FirstName = "User",
                        LastName = "1",
                        EmploymentDate = DateTime.UtcNow
                    },
                    EmployeeId = "User1",
                    OrganizationId = 2,
                    Points = 0,
                    MultiplyBy = 0,
                    Status = KudosStatus.Approved,
                    Created = DateTime.UtcNow
                },
                new KudosLog // 0 (KudosType = Minus)
                {
                    KudosTypeName = "Minus",
                    KudosTypeValue = 2,
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Minus,
                    Employee = new ApplicationUser
                    {
                        Id = "User1",
                        FirstName = "User",
                        LastName = "1",
                        EmploymentDate = DateTime.UtcNow
                    },
                    EmployeeId = "User1",
                    OrganizationId = 2,
                    Points = 10,
                    MultiplyBy = 5,
                    Status = KudosStatus.Approved,
                    Created = DateTime.UtcNow
                },
                new KudosLog // 0 (Status = Pending)
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    Employee = new ApplicationUser
                    {
                        Id = "User1",
                        FirstName = "User",
                        LastName = "1",
                        EmploymentDate = DateTime.UtcNow
                    },
                    EmployeeId = "User1",
                    OrganizationId = 2,
                    Points = 10.1M,
                    MultiplyBy = 24,
                    Status = KudosStatus.Pending,
                    Created = DateTime.UtcNow
                },
                new KudosLog // +242.4
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    Employee = new ApplicationUser
                    {
                        Id = "User1",
                        FirstName = "User",
                        LastName = "1",
                        EmploymentDate = DateTime.UtcNow
                    },
                    EmployeeId = "User1",
                    OrganizationId = 2,
                    Points = 242.4M,
                    MultiplyBy = 121,
                    Status = KudosStatus.Approved,
                    Created = DateTime.UtcNow
                },

                // User2
                new KudosLog // +20
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    Employee = new ApplicationUser
                    {
                        Id = "User2",
                        FirstName = "User",
                        LastName = "2",
                        EmploymentDate = DateTime.UtcNow
                    },
                    EmployeeId = "User2",
                    OrganizationId = 2,
                    Points = 20,
                    MultiplyBy = 10,
                    Status = KudosStatus.Approved,
                    Created = DateTime.UtcNow
                },
                new KudosLog // 0 (OrgID = 1)
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    Employee = new ApplicationUser
                    {
                        Id = "User2",
                        FirstName = "User",
                        LastName = "2",
                        EmploymentDate = DateTime.UtcNow
                    },
                    EmployeeId = "User2",
                    OrganizationId = 1,
                    Points = 10,
                    MultiplyBy = 2,
                    Status = KudosStatus.Approved,
                    Created = DateTime.UtcNow
                },

                // User3
                new KudosLog // +34
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    Employee = new ApplicationUser
                    {
                        Id = "User3",
                        FirstName = "User",
                        LastName = "3",
                        EmploymentDate = DateTime.UtcNow
                    },
                    EmployeeId = "User3",
                    OrganizationId = 2,
                    Points = 34,
                    MultiplyBy = 17,
                    Status = KudosStatus.Approved,
                    Created = DateTime.UtcNow
                },

                // User 4 - to test DateTime dimension.
                new KudosLog // 0 (DateTime out of range)
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    Employee = new ApplicationUser
                    {
                        Id = "User4",
                        FirstName = "User",
                        LastName = "4",
                        EmploymentDate = DateTime.UtcNow
                    },
                    EmployeeId = "User4",
                    OrganizationId = 2,
                    Points = 300,
                    MultiplyBy = 150,
                    Status = KudosStatus.Approved,
                    Created = DateTime.UtcNow.AddMonths(-4)
                },
                new KudosLog // +10
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    Employee = new ApplicationUser
                    {
                        Id = "User4",
                        FirstName = "User",
                        LastName = "4",
                        EmploymentDate = DateTime.UtcNow.AddMonths(-2)
                    },
                    EmployeeId = "User4",
                    OrganizationId = 2,
                    Points = 10,
                    MultiplyBy = 5,
                    Status = KudosStatus.Approved,
                    Created = DateTime.UtcNow.AddMonths(-2)
                }
            };

            var users = new List<ApplicationUser>()
            {
                new ApplicationUser
                {
                    Id = "User1",
                    FirstName = "User",
                    LastName = "1",
                    EmploymentDate = DateTime.UtcNow
                },
                new ApplicationUser
                {
                    Id = "User2",
                    FirstName = "User",
                    LastName = "2",
                    EmploymentDate = DateTime.UtcNow
                },
                new ApplicationUser
                {
                    Id = "User3",
                    FirstName = "User",
                    LastName = "3",
                    EmploymentDate = DateTime.UtcNow
                },
                new ApplicationUser
                {
                    Id = "User4",
                    FirstName = "User",
                    LastName = "4",
                    EmploymentDate = DateTime.UtcNow
                },
            };

            _usersDbSet.SetDbSetData(users.AsQueryable());
            _kudosLogsDbSet.SetDbSetData(kudosLogs.AsQueryable());
        }

        private void MockKudosLogsForProfileUpdate()
        {
            var kudosLogs = new List<KudosLog>
            {
                //should ingore pending
                new KudosLog
                {
                    Status = KudosStatus.Pending,
                    Id = 1,
                    EmployeeId = "Id",
                    KudosTypeName = "Type1",
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    OrganizationId = 1,
                    Created = DateTime.UtcNow,
                    CreatedBy = "testUserId"
                },

                //should ingore organizationId
                new KudosLog
                {
                    Status = KudosStatus.Approved,
                    Id = 2,
                    EmployeeId = "Id",
                    KudosTypeName = "Type1",
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    OrganizationId = 2,
                    Created = DateTime.UtcNow,
                    Points = 1
                },

                //Should ignore because of employment date
                new KudosLog
                {
                    Status = KudosStatus.Approved,
                    Id = 3,
                    EmployeeId = "Id",
                    KudosTypeName = "Type2",
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    KudosTypeValue = 2,
                    OrganizationId = 1,
                    Created = DateTime.UtcNow.AddDays(-11),
                    Points = 1
                },
                new KudosLog
                {
                    Status = KudosStatus.Approved,
                    Id = 4,
                    EmployeeId = "Id",
                    KudosTypeName = "Type2",
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    OrganizationId = 1,
                    Created = DateTime.UtcNow,
                    Points = 10
                },
                new KudosLog
                {
                    Status = KudosStatus.Approved,
                    Id = 5,
                    EmployeeId = "Id",
                    KudosTypeName = "Type2",
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Ordinary,
                    OrganizationId = 1,
                    Created = DateTime.UtcNow,
                    Points = 1
                },
                new KudosLog
                {
                    Status = KudosStatus.Approved,
                    Id = 6,
                    EmployeeId = "Id",
                    KudosTypeName = "Minus",
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Minus,
                    OrganizationId = 1,
                    Created = DateTime.UtcNow,
                    Points = 1
                },
                new KudosLog
                {
                    Status = KudosStatus.Approved,
                    Id = 6,
                    EmployeeId = "Id",
                    KudosTypeName = "Other",
                    KudosSystemType = ConstBusinessLayer.KudosTypeEnum.Other,
                    OrganizationId = 1,
                    Created = DateTime.UtcNow,
                    Points = 1,
                    KudosBasketId = 1
                },
            };
            _kudosLogsDbSet.SetDbSetData(kudosLogs.AsQueryable());
        }
        #endregion
    }
}
