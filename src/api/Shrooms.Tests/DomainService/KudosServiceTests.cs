using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Kudos;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.DataTransferObjects.Wall.Likes;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Exceptions.Exceptions.Kudos;
using Shrooms.Domain.Services.FilterPresets;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.ServiceValidators.Validators.Kudos;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.DomainService
{
    public class KudosServiceTests
    {
        private IKudosService _kudosService;
        private DbSet<KudosLog> _kudosLogsDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<KudosType> _kudosTypesDbSet;
        private DbSet<Organization> _organizationDbSet;
        private IFilterPresetService _filterPresetService;
        private IUnitOfWork2 _uow;
        private IMapper _mapper;
        private IApplicationSettings _settings;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _mapper = ModelMapper.Create();
            _settings = Substitute.For<IApplicationSettings>();
            _settings.KudosAvailableToSendPerMonth.Returns((int?)50);

            _kudosLogsDbSet = Substitute.For<DbSet<KudosLog>, IQueryable<KudosLog>, IAsyncEnumerable<KudosLog>>();
            _kudosLogsDbSet.SetDbSetDataForAsync(MockKudosLogs());

            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            _usersDbSet.SetDbSetDataForAsync(MockUsers());

            _kudosTypesDbSet = Substitute.For<DbSet<KudosType>, IQueryable<KudosType>, IAsyncEnumerable<KudosType>>();
            _kudosTypesDbSet.SetDbSetDataForAsync(MockKudosTypes());

            _organizationDbSet = Substitute.For<DbSet<Organization>, IQueryable<Organization>, IAsyncEnumerable<Organization>>();
            _organizationDbSet.SetDbSetDataForAsync(MockOrganization());

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

            _filterPresetService = Substitute.For<IFilterPresetService>();

            MockRoleService(roleService);

            _kudosService = new KudosService(
                _uow,
                uow2,
                _mapper,
                permissionService,
                kudosServiceValidation,
                asyncRunner,
                _filterPresetService,
                _settings);
        }

        #region GetKudosLogs

        [Test]
        public async Task Should_Return_All_Kudos_Logs()
        {
            var filter = new KudosLogsFilterDto
            {
                OrganizationId = 2,
                Page = 1,
                Status = BusinessLayerConstants.KudosStatusAllFilter,
                UserId = "testUserId",
                SortBy = "Created",
                SortOrder = "desc"
            };

            var result = await _kudosService.GetKudosLogsAsync(filter);

            Assert.That(result.TotalKudosCount, Is.EqualTo(4));
        }

        [Test]
        public async Task Should_Return_Kudos_Logs_Filtered_By_Status_Approved()
        {
            var filter = new KudosLogsFilterDto
            {
                OrganizationId = 2,
                Page = 1,
                Status = "Approved",
                UserId = "testUserId",
                SortBy = "Created",
                SortOrder = "desc"
            };

            var result = await _kudosService.GetKudosLogsAsync(filter);

            Assert.That(result.TotalKudosCount, Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Return_Return_All_Kudos_Logs_With_Organization_Filter()
        {
            MockKudosLogsForOrganizationTest();
            var filter = new KudosLogsFilterDto
            {
                OrganizationId = 2,
                Page = 1,
                Status = BusinessLayerConstants.KudosStatusAllFilter,
                SortBy = "Created",
                SortOrder = "desc"
            };

            var result = await _kudosService.GetKudosLogsAsync(filter);

            Assert.That(result.KudosLogs.Count(), Is.EqualTo(1));
            Assert.That(result.KudosLogs.First().Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Return_Specific_User_Kudos_Logs()
        {
            var result = await _kudosService.GetUserKudosLogsAsync("testUserId", 1, 2);

            Assert.That(result.TotalKudosCount, Is.EqualTo(3));
        }

        [Test]
        public async Task Should_Return_Specific_User_Kudos_Logs_With_Organization_Filter()
        {
            MockKudosLogsForOrganizationTest();
            var result = await _kudosService.GetUserKudosLogsAsync("UserId", 1, 1);

            Assert.That(result.KudosLogs.Count(), Is.EqualTo(1));
            Assert.That(result.KudosLogs.First().Id, Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Return_Last_Five_Approved_Kudos_Logs()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2
            };

            var result = await _kudosService.GetLastKudosLogsForWallAsync(userAndOrg);

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Return_Kudos_Logs_For_Wall_With_Organization_Filter()
        {
            MockKudosLogsForOrganizationTest();
            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "UserId"
            };

            var result = (await _kudosService.GetLastKudosLogsForWallAsync(userAndOrg)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Comment, Is.EqualTo("Comment2"));
        }

        [Test]
        public async Task Should_Return_Wall_Kudos_Logs_With_Id_And_Likes()
        {
            var log = new KudosLog
            {
                Status = KudosStatus.Approved,
                Id = 10,
                EmployeeId = "testUserId",
                Employee = new ApplicationUser
                {
                    Id = "testUserId",
                    FirstName = "name",
                    LastName = "surname"
                },
                KudosTypeName = "Type2",
                KudosSystemType = KudosTypeEnum.Ordinary,
                OrganizationId = 2,
                CreatedBy = "testUserId2",
                Comments = "With likes",
                Likes = new LikesCollection
                {
                    new Like("testUserId3", LikeTypeEnum.Congrats),
                    new Like("deletedUserId", LikeTypeEnum.Like)
                }
            };

            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog> { log }.AsQueryable());

            var userAndOrg = new UserAndOrganizationDto { OrganizationId = 2 };

            var result = (await _kudosService.GetLastKudosLogsForWallAsync(userAndOrg)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(10));
            // Likes from users that no longer exist are filtered out.
            var likes = result.First().Likes.ToList();
            Assert.That(likes.Count, Is.EqualTo(1));
            Assert.That(likes.First().UserId, Is.EqualTo("testUserId3"));
            Assert.That(likes.First().Type, Is.EqualTo(LikeTypeEnum.Congrats));
            Assert.That(likes.First().FullName, Is.Not.Empty);
        }

        [Test]
        public async Task Should_Like_Kudos_Log()
        {
            var log = ApprovedKudosLogForLikes();
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog> { log }.AsQueryable());

            await _kudosService.ToggleLikeAsync(new AddLikeDto { Id = 10, Type = LikeTypeEnum.Like },
                new UserAndOrganizationDto { UserId = "testUserId3", OrganizationId = 2 });

            Assert.That(log.Likes.Count, Is.EqualTo(1));
            Assert.That(log.Likes.First().UserId, Is.EqualTo("testUserId3"));
        }

        [Test]
        public async Task Should_Unlike_Kudos_Log()
        {
            var log = ApprovedKudosLogForLikes();
            log.Likes = new LikesCollection { new Like("testUserId3", LikeTypeEnum.Like) };
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog> { log }.AsQueryable());

            await _kudosService.ToggleLikeAsync(new AddLikeDto { Id = 10, Type = LikeTypeEnum.Like },
                new UserAndOrganizationDto { UserId = "testUserId3", OrganizationId = 2 });

            Assert.That(log.Likes.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task Should_Add_Second_Kudos_Log_Reaction_Of_Different_Type()
        {
            var log = ApprovedKudosLogForLikes();
            log.Likes = new LikesCollection { new Like("testUserId3", LikeTypeEnum.Like) };
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog> { log }.AsQueryable());

            await _kudosService.ToggleLikeAsync(new AddLikeDto { Id = 10, Type = LikeTypeEnum.Love },
                new UserAndOrganizationDto { UserId = "testUserId3", OrganizationId = 2 });

            Assert.That(log.Likes.Count, Is.EqualTo(2));
        }

        [Test]
        public void Should_Throw_When_Liking_Nonexistent_Kudos_Log()
        {
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog>().AsQueryable());

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _kudosService.ToggleLikeAsync(new AddLikeDto { Id = 10, Type = LikeTypeEnum.Like },
                    new UserAndOrganizationDto { UserId = "testUserId3", OrganizationId = 2 }));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        [Test]
        public void Should_Throw_When_Liking_Pending_Kudos_Log()
        {
            var log = ApprovedKudosLogForLikes();
            log.Status = KudosStatus.Pending;
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog> { log }.AsQueryable());

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _kudosService.ToggleLikeAsync(new AddLikeDto { Id = 10, Type = LikeTypeEnum.Like },
                    new UserAndOrganizationDto { UserId = "testUserId3", OrganizationId = 2 }));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        [Test]
        public void Should_Throw_When_Liking_Kudos_Log_From_Other_Organization()
        {
            var log = ApprovedKudosLogForLikes();
            _kudosLogsDbSet.SetDbSetDataForAsync(new List<KudosLog> { log }.AsQueryable());

            var ex = Assert.ThrowsAsync<ValidationException>(async () =>
                await _kudosService.ToggleLikeAsync(new AddLikeDto { Id = 10, Type = LikeTypeEnum.Like },
                    new UserAndOrganizationDto { UserId = "testUserId3", OrganizationId = 1 }));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.ContentDoesNotExist));
        }

        private static KudosLog ApprovedKudosLogForLikes()
        {
            return new KudosLog
            {
                Status = KudosStatus.Approved,
                Id = 10,
                EmployeeId = "testUserId",
                KudosTypeName = "Type2",
                KudosSystemType = KudosTypeEnum.Ordinary,
                OrganizationId = 2,
                CreatedBy = "testUserId2"
            };
        }

        [Test]
        public async Task Should_Return_Kudos_Logs_For_Pie_Chart_With_Organization_Filter()
        {
            MockKudosLogsForPieChart();
            var result = (await _kudosService.GetKudosPieChartDataAsync(1, "UserId")).ToList();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Value, Is.EqualTo(3));
        }

        [Test]
        public async Task Should_Return_Kudos_Logs_For_Pie_Chart_With_Organization_Filter_2()
        {
            MockKudosLogsForPieChart();
            var result = (await _kudosService.GetKudosPieChartDataAsync(2, "UserId")).ToList();
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.First().Value, Is.EqualTo(2));
            Assert.That(result.ToArray()[1].Value, Is.EqualTo(10));
            Assert.That(result.ToArray()[1].Name, Is.EqualTo("Type2"));
        }

        [Test]
        public async Task Should_Return_Approved_Kudos_Logs_With_Organization_Filter()
        {
            await _usersDbSet.FindAsync("CreatedUserId");

            MockKudosLogsForApprovedList();
            var result = (await _kudosService.GetApprovedKudosListAsync("UserId", 1)).ToList();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Comments, Is.EqualTo("Comment2"));
            Assert.That(result.First().Sender.Id, Is.EqualTo("CreatedUserId"));
        }

        [Test]
        public async Task Should_Return_Approved_Kudos_Logs_With_Organization_Filter_2()
        {
            MockKudosLogsForApprovedList();
            var result = (await _kudosService.GetApprovedKudosListAsync("UserId", 2)).ToList();
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.First().Comments, Is.EqualTo("Comment1"));
            Assert.That(result.First().Sender.Id, Is.EqualTo("CreatedUserId"));
            Assert.That(result.ToArray()[1].Comments, Is.EqualTo("Comment3"));
        }

        #endregion

        #region GetKudosTypes

        [Test]
        public async Task Should_Return_All_Kudos_Types()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                UserId = "testUserId2"
            };

            var types = await _kudosService.GetKudosTypesAsync(userAndOrg);
            Assert.That(types.Count(), Is.EqualTo(5));
        }

        [Test]
        public async Task Should_Return_Basic_Kudos_Types()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                UserId = "testUserId"
            };

            var types = await _kudosService.GetKudosTypesAsync(userAndOrg);
            Assert.That(types.Count(), Is.EqualTo(5));
        }

        [Test]
        public async Task Should_Return_Active_Kudos_Types()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                UserId = "testUserId"
            };

            var types = await _kudosService.GetKudosTypesAsync(userAndOrg);
            Assert.That(types.Any(type => type.IsActive), Is.True);
        }

        #endregion

        #region UpdateKudosLogs

        [Test]
        public void Should_Throw_When_Approving_Other_Organization_Log()
        {
            MockKudosLogsForUpdate();

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "UserId"
            };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _kudosService.ApproveKudosAsync(1, userAndOrg));
        }

        [Test]
        public async Task Should_Return_When_Kudos_Log_Was_Not_Approved()
        {
            MockKudosLogsForUpdate();

            var userAndOrg = new UserAndOrganizationDto
            {
                OrganizationId = 2,
                UserId = "UserId2"
            };

            await _kudosService.ApproveKudosAsync(1, userAndOrg);
            Assert.That(_kudosLogsDbSet.First().Employee.RemainingKudos, Is.EqualTo(_kudosLogsDbSet.First().Points));
            Assert.That(_kudosLogsDbSet.First().Status, Is.EqualTo(KudosStatus.Approved));
        }

        [Test]
        public async Task Should_Return_If_Rejection_Doesnt_Update_Status()
        {
            var kudosRejectDto = new KudosRejectDto
            {
                OrganizationId = 2,
                UserId = "testUserId",
                Id = 1,
                KudosRejectionMessage = "testMessage"
            };

            await _kudosService.RejectKudosAsync(kudosRejectDto);

            var log = await _kudosLogsDbSet.FirstAsync(x => x.Id == 1);
            Assert.That(log.Status, Is.EqualTo(KudosStatus.Rejected));
            Assert.That(log.RejectionMessage, Is.EqualTo("testMessage"));
        }

        [Test]
        public void Should_Throw_When_Rejecting_Other_Organization_Log()
        {
            MockKudosLogsForUpdate();

            var kudosRejectDto = new KudosRejectDto
            {
                OrganizationId = 1,
                UserId = "testUserId",
                Id = 1,
                KudosRejectionMessage = "testMessage"
            };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _kudosService.RejectKudosAsync(kudosRejectDto));
        }

        [Test]
        public void Should_Update_User_Profile_Kudos_Depending_On_Logs()
        {
            var user = new ApplicationUser
            {
                Id = "Id",
                EmploymentDate = DateTime.UtcNow.AddDays(-10)
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "userId"
            };

            MockKudosLogsForProfileUpdate();

            _kudosService.UpdateProfileKudosAsync(user, userOrg);
            Assert.That(user.TotalKudos, Is.EqualTo(11));
            Assert.That(user.RemainingKudos, Is.EqualTo(9));
            Assert.That(user.SpentKudos, Is.EqualTo(2));
        }

        #endregion

        #region AddKudosLogs

        //Checks if permission validation for minus kudos operation works properly.
        [Test]
        public void Should_Return_If_User_Has_No_Permission()
        {
            var kudosLog = new AddKudosLogDto
            {
                OrganizationId = 2,
                PointsTypeId = 1,
                UserId = "testUserId",
                ReceivingUserIds = new List<string> { "testUserId" },
                MultiplyBy = 2,
                Comment = "Comment",
                IsActive = true
            };

            Assert.ThrowsAsync<KudosException>(async () => await _kudosService.AddKudosLogAsync(kudosLog));
        }

        //Checks if kudos logs has been saved for kudos minus operation.
        [Test]
        public async Task Should_Return_If_Kudos_Logs_Has_Not_Been_Saved_1()
        {
            var kudosLog = new AddKudosLogDto
            {
                OrganizationId = 2,
                PointsTypeId = 5,
                UserId = "testUserId2",
                ReceivingUserIds = new List<string> { "testUserId3", "testUserId4" },
                MultiplyBy = 2,
                Comment = "Comment",
                IsActive = true
            };

            await _kudosService.AddKudosLogAsync(kudosLog);
            _kudosLogsDbSet.Received(2).Add(Arg.Any<KudosLog>());
            await _uow.Received(1).SaveChangesAsync(false);
        }

        [Test]
        public void AddKudosLog_OverridenPointsAmountPassed_SaveTriggeredWithExplicitAmount()
        {
            // Arrange
            const int explicitAmount = 1234564;
            var kudosLog = new AddKudosLogDto
            {
                OrganizationId = 2,
                PointsTypeId = 2,
                UserId = "testUserId2",
                ReceivingUserIds = new List<string> { "testUserId3", "testUserId4" },
                MultiplyBy = 2,
                Comment = "Comment",
                IsActive = true
            };

            // Act
            _kudosService.AddKudosLogAsync(kudosLog, explicitAmount);

            // Assert
            _kudosLogsDbSet.Received(4).Add(Arg.Is<KudosLog>(l => l.Points == explicitAmount));
        }

        //Checks if kudos logs has been saved for kudos send operation.
        [Test]
        public async Task Should_Return_If_Kudos_Logs_Has_Not_Been_Saved_2()
        {
            var kudosLog = new AddKudosLogDto
            {
                OrganizationId = 2,
                PointsTypeId = 2,
                UserId = "testUserId2",
                ReceivingUserIds = new List<string> { "testUserId3", "testUserId4" },
                MultiplyBy = 2,
                Comment = "Comment",
                IsActive = true
            };

            await _kudosService.AddKudosLogAsync(kudosLog);
            _kudosLogsDbSet.Received(4).Add(Arg.Any<KudosLog>());
            await _uow.Received(1).SaveChangesAsync(false);
        }

        //Checks if available kudos validation for send kudos operation works properly.
        [Test]
        public void Should_Return_If_User_Has_No_Available_Kudos()
        {
            _kudosTypesDbSet.SetDbSetDataForAsync(MockKudosTypes());
            _kudosLogsDbSet.SetDbSetDataForAsync(MockKudosLogs());

            var kudosLog = new AddKudosLogDto
            {
                OrganizationId = 2,
                PointsTypeId = 2,
                UserId = "testUserId2",
                ReceivingUserIds = new List<string> { "testUserId3", "testUserId4", "testUserId" },
                MultiplyBy = 2,
                Comment = "Comment",
                IsActive = true
            };

            Assert.ThrowsAsync<KudosException>(async () => await _kudosService.AddKudosLogAsync(kudosLog));
        }

        [Test]
        public void Should_Return_When_User_Sends_Kudos_To_Himself()
        {
            var kudosLog = new AddKudosLogDto
            {
                OrganizationId = 2,
                PointsTypeId = 2,
                UserId = "testUserId2",
                ReceivingUserIds = new List<string> { "testUserId3", "testUserId2" },
                MultiplyBy = 1,
                Comment = "Comment",
                IsActive = true
            };

            Assert.ThrowsAsync<KudosException>(async () => await _kudosService.AddKudosLogAsync(kudosLog));
        }

        //User can send limited amount of kudos per month, so this test
        //checks if validation for monthly sending kudos works properly.
        [Test]
        public void Should_Return_If_User_Has_No_Available_Monthly_Kudos()
        {
            var kudosLog = new AddKudosLogDto
            {
                OrganizationId = 2,
                PointsTypeId = 2,
                UserId = "testUserId5",
                ReceivingUserIds = new List<string> { "testUserId3", "testUserId4", "testUserId" },
                MultiplyBy = 2,
                Comment = "Comment",
                IsActive = true
            };

            Assert.ThrowsAsync<KudosException>(async () => await _kudosService.AddKudosLogAsync(kudosLog));
        }

        //Checks if kudos logs has been saved for kudos add operation.
        [Test]
        public async Task Should_Return_If_Kudos_Logs_Has_Not_Been_Saved_3()
        {
            var kudosLog = new AddKudosLogDto
            {
                OrganizationId = 2,
                PointsTypeId = 3,
                UserId = "testUserId2",
                ReceivingUserIds = new List<string> { "testUserId3", "testUserId4" },
                MultiplyBy = 2,
                Comment = "Comment",
                IsActive = true
            };

            await _kudosService.AddKudosLogAsync(kudosLog);
            _kudosLogsDbSet.Received(2).Add(Arg.Any<KudosLog>());
            await _uow.Received(1).SaveChangesAsync(false);
        }

        #endregion

        #region GetKudosStats

        [Test]
        public async Task Should_Return_Correctly_Summed_Kudos()
        {
            MockKudosLogsForStats();
            var actual = (await _kudosService.GetKudosStatsAsync(3, 10, 2)).ToList();
            Assert.That(actual[0].Name, Is.EqualTo("User 1"));
            Assert.That(actual[0].KudosAmount, Is.EqualTo(274.4));
            Assert.That(actual[1].Name, Is.EqualTo("User 3"));
            Assert.That(actual[1].KudosAmount, Is.EqualTo(34));
            Assert.That(actual[2].Name, Is.EqualTo("User 2"));
            Assert.That(actual[2].KudosAmount, Is.EqualTo(20));
            Assert.That(actual[3].Name, Is.EqualTo("User 4"));
            Assert.That(actual[3].KudosAmount, Is.EqualTo(10));
        }

        #endregion

        #region HasPendingKudos

        [Test]
        public async Task HasPendingKudos_Should_Return_True()
        {
            MockKudosLogsForStats();
            var actual = await _kudosService.HasPendingKudosAsync("User1");
            Assert.That(actual, Is.EqualTo(true));
        }

        [Test]
        public async Task HasPendingKudos_Should_Return_False()
        {
            MockKudosLogsForStats();
            var actual = await _kudosService.HasPendingKudosAsync("User2");
            Assert.That(actual, Is.EqualTo(false));
        }

        #endregion

        #region GetKudosTypeSend

        [Test]
        public async Task GetKudosTypeSend_FromKudosTypes_ReturnsOnlySendType()
        {
            var userAndOrg = new UserAndOrganizationDto
            {
                UserId = "testUserId2"
            };

            var result = await _kudosService.GetSendKudosTypeAsync(userAndOrg);

            Assert.That(KudosTypeEnum.Send, Is.EqualTo(result.Type));
        }

        #endregion

        #region RemoveKudosType

        [Test]
        public async Task RemoveKudosType_WhenValid_RemovesTypeFromPresets()
        {
            // Arrange
            const int id = 1;

            var userOrg = new UserAndOrganizationDto
            {
                UserId = "Id"
            };

            _kudosTypesDbSet.SetDbSetDataForAsync(new List<KudosType>
            {
                new()
                {
                    Id = id,
                    Name = "Test"
                }
            });

            // Act
            await _kudosService.RemoveKudosTypeAsync(id, userOrg);

            // Assert
            await _filterPresetService
                .Received(1)
                .RemoveDeletedTypeFromPresetsAsync(Arg.Is(id.ToString()), Arg.Is(FilterType.Kudos), Arg.Is(0));
        }

        #endregion

        #region CreateKudosType

        [Test]
        public void CreateKudosType_WhenNameAlreadyExists_ThrowsValidationException()
        {
            var dto = new NewKudosTypeDto
            {
                Name = "anythingelse",
                Multiplier = 5,
                UserId = "testUserId2",
                OrganizationId = 2
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _kudosService.CreateKudosTypeAsync(dto));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.KudosTypeNameAlreadyExists));
            _kudosTypesDbSet.DidNotReceiveWithAnyArgs().Add(default);
        }

        [Test]
        public async Task CreateKudosType_WhenNameIsUnique_AddsType()
        {
            var dto = new NewKudosTypeDto
            {
                Name = "BrandNewType",
                Multiplier = 5,
                UserId = "testUserId2",
                OrganizationId = 2
            };

            await _kudosService.CreateKudosTypeAsync(dto);

            _kudosTypesDbSet.Received(1).Add(Arg.Is<KudosType>(type => type.Name == dto.Name));
            await _uow.Received(1).SaveChangesAsync(dto.UserId);
        }

        #endregion

        #region UpdateKudosType

        [Test]
        public void UpdateKudosType_WhenNameBelongsToAnotherType_ThrowsValidationException()
        {
            var dto = new KudosTypeDto
            {
                Id = 3,
                Name = "Active",
                Value = 2,
                UserId = "testUserId2",
                OrganizationId = 2
            };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _kudosService.UpdateKudosTypeAsync(dto));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.KudosTypeNameAlreadyExists));
            _uow.DidNotReceive().SaveChangesAsync(Arg.Any<string>());
        }

        [Test]
        public async Task UpdateKudosType_WhenNameBelongsToSameType_UpdatesType()
        {
            var dto = new KudosTypeDto
            {
                Id = 3,
                Name = "AnythingElse",
                Value = 7,
                Description = "Updated description",
                IsActive = true,
                UserId = "testUserId2",
                OrganizationId = 2
            };

            await _kudosService.UpdateKudosTypeAsync(dto);

            var updatedType = ((IQueryable<KudosType>)_kudosTypesDbSet).First(type => type.Id == dto.Id);

            Assert.That(updatedType.Value, Is.EqualTo(dto.Value));
            Assert.That(updatedType.Description, Is.EqualTo(dto.Description));
            await _uow.Received(1).SaveChangesAsync(dto.UserId);
        }

        #endregion

        #region MockData

        private static void MockRoleService(IRoleService roleService)
        {
            var newRoleId = Guid.NewGuid().ToString();
            roleService.GetRoleIdByNameAsync(Roles.NewUser).Returns(newRoleId);
            roleService.ExcludeUsersWithRole(newRoleId).ReturnsForAnyArgs(x => true);
        }

        private static IPermissionService MockPermissionService()
        {
            var permissionService = Substitute.For<IPermissionService>();

            permissionService.UserHasPermissionAsync(Arg.Is<UserAndOrganizationDto>(x => x.UserId == "testUserId"), Arg.Any<string>()).Returns(false);
            permissionService.UserHasPermissionAsync(Arg.Is<UserAndOrganizationDto>(x => x.UserId == "testUserId2"), Arg.Any<string>()).Returns(true);
            return permissionService;
        }

        private static IKudosServiceValidator MockServiceValidator()
        {
            var kudosServiceValidation = Substitute.For<IKudosServiceValidator>();
            kudosServiceValidation
                .When(x => x.ValidateKudosMinusPermission(false))
                .Do(_ => throw new KudosException(""));

            kudosServiceValidation
                .When(x => x.ValidateUserAvailableKudos(4, 6))
                .Do(_ => throw new KudosException(""));

            kudosServiceValidation
                .When(x => x.ValidateUserAvailableKudosToSendPerMonth(2, 0))
                .Do(_ => throw new KudosException(""));

            kudosServiceValidation
                .When(x => x.ValidateSendingToSameUserAsReceiving("testUserId2", "testUserId2"))
                .Do(_ => throw new KudosException(""));

            return kudosServiceValidation;
        }

        private void MockFindMethod()
        {
            _organizationDbSet.FindAsync(2).Returns(MockOrganization().FirstOrDefault(x => x.Id == 2));

            _kudosTypesDbSet.FindAsync(1).Returns(MockKudosTypes().FirstOrDefault(x => x.Id == 1));
            _kudosTypesDbSet.FindAsync(2).Returns(MockKudosTypes().FirstOrDefault(x => x.Id == 2));
            _kudosTypesDbSet.FindAsync(3).Returns(MockKudosTypes().FirstOrDefault(x => x.Id == 3));
            _kudosTypesDbSet.FindAsync(5).Returns(MockKudosTypes().FirstOrDefault(x => x.Id == 5));

            _usersDbSet.FindAsync("testUserId").Returns(MockUsers().FirstOrDefault(x => x.Id == "testUserId"));
            _usersDbSet.FindAsync("testUserId2").Returns(MockUsers().FirstOrDefault(x => x.Id == "testUserId2"));
            _usersDbSet.FindAsync("testUserId3").Returns(MockUsers().FirstOrDefault(x => x.Id == "testUserId3"));
            _usersDbSet.FindAsync("testUserId4").Returns(MockUsers().FirstOrDefault(x => x.Id == "testUserId4"));
            _usersDbSet.FindAsync("testUserId5").Returns(MockUsers().FirstOrDefault(x => x.Id == "testUserId5"));
            _usersDbSet.FindAsync("CreatedUserId").Returns(MockUsers().FirstOrDefault(x => x.Id == "CreatedUserId"));
        }

        private IQueryable<Organization> MockOrganization()
        {
            return new List<Organization>
            {
                new()
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
                new()
                {
                    Id = "testUserId",
                    FirstName = "name",
                    LastName = "surname",
                    OrganizationId = 2,
                    CultureCode = "en-US"
                },
                new()
                {
                    Id = "testUserId2",
                    TotalKudos = 4,
                    SpentKudos = 0,
                    RemainingKudos = 4,
                    OrganizationId = 2,
                    CultureCode = "en-US"
                },
                new()
                {
                    Id = "testUserId3",
                    TotalKudos = 10,
                    SpentKudos = 2,
                    RemainingKudos = 8,
                    OrganizationId = 2
                },
                new()
                {
                    Id = "testUserId4",
                    TotalKudos = 2,
                    SpentKudos = 0,
                    RemainingKudos = 2,
                    OrganizationId = 2
                },
                new()
                {
                    Id = "testUserId5",
                    TotalKudos = 12,
                    SpentKudos = 2,
                    RemainingKudos = 10,
                    OrganizationId = 2
                },
                new()
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
                new()
                {
                    Id = 1,
                    Name = "Minus",
                    Value = 1,
                    Type = KudosTypeEnum.Minus,
                    IsActive = true
                },
                new()
                {
                    Id = 2,
                    Name = "Send",
                    Value = 1,
                    Type = KudosTypeEnum.Send,
                    IsActive = true
                },
                new()
                {
                    Id = 3,
                    Name = "AnythingElse",
                    Value = 2,
                    Type = KudosTypeEnum.Ordinary,
                    IsActive = true
                },
                new()
                {
                    Id = 4,
                    Name = "Other",
                    Value = 3,
                    Type = KudosTypeEnum.Other
                },
                new()
                {
                    Id = 5,
                    Name = "Active",
                    Value = 1,
                    Type = KudosTypeEnum.Ordinary,
                    IsActive = true
                }
            }.AsQueryable();
        }

        private IQueryable<KudosLog> MockKudosLogs()
        {
            var kudosAvailableToSendPerMonth = _settings.KudosAvailableToSendPerMonth ?? BusinessLayerConstants.DefaultKudosAvailableToSendPerMonth;
            var kudosLogs = new List<KudosLog>
            {
                new()
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
                    KudosSystemType = KudosTypeEnum.Ordinary,
                    OrganizationId = 2,
                    CreatedBy = "testUserId"
                },
                new()
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
                    KudosSystemType = KudosTypeEnum.Ordinary,
                    OrganizationId = 2,
                    CreatedBy = "testUserId"
                },
                new()
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
                    KudosSystemType = KudosTypeEnum.Ordinary,
                    OrganizationId = 2,
                    CreatedBy = "testUserId",
                    Points = 0,
                    Comments = "Hello"
                },
                new()
                {
                    Status = KudosStatus.Approved,
                    Id = 4,
                    EmployeeId = "testUserId3",
                    KudosTypeName = "Send",
                    KudosSystemType = KudosTypeEnum.Send,
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name",
                        LastName = "surname"
                    },
                    OrganizationId = 2,
                    CreatedBy = "testUserId5",
                    MultiplyBy = kudosAvailableToSendPerMonth - 2,
                    Created = DateTime.UtcNow,
                    Points = kudosAvailableToSendPerMonth - 2,
                    Comments = "Hello"
                }
            };
            return kudosLogs.AsQueryable();
        }

        private void MockKudosLogsForOrganizationTest()
        {
            var kudosLogs = new List<KudosLog>
            {
                new()
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
                new()
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
                new()
                {
                    Id = "CreatedUserId"
                }
            };

            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());
            _kudosLogsDbSet.SetDbSetDataForAsync(kudosLogs.AsQueryable());
        }

        private void MockKudosLogsForPieChart()
        {
            var kudosLogs = new List<KudosLog>
            {
                new()
                {
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    EmployeeId = "UserId",
                    OrganizationId = 2,
                    MultiplyBy = 1,
                    Points = 2,
                    Status = KudosStatus.Approved
                },
                new()
                {
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    EmployeeId = "UserId",
                    OrganizationId = 1,
                    Points = 3,
                    Status = KudosStatus.Approved
                },
                new()
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    EmployeeId = "UserId",
                    OrganizationId = 2,
                    Points = 10,
                    Status = KudosStatus.Approved
                }
            };
            _kudosLogsDbSet.SetDbSetDataForAsync(kudosLogs.AsQueryable());
        }

        private void MockKudosLogsForApprovedList()
        {
            var kudosLogs = new List<KudosLog>
            {
                new()
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
                new()
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
                new()
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
                new()
                {
                    Id = "CreatedUserId"
                }
            };

            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());
            _kudosLogsDbSet.SetDbSetDataForAsync(kudosLogs.AsQueryable());
        }

        private void MockKudosLogsForUpdate()
        {
            var kudosLogs = new List<KudosLog>
            {
                new()
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
                new()
                {
                    Id = 2,
                    ShortName = "VismaShortName"
                }
            };

            var users = new List<ApplicationUser>
            {
                new()
                {
                    Id = "CreatedUserId",
                    FirstName = "Name",
                    LastName = "Surname",
                    TotalKudos = 0,
                    RemainingKudos = 0
                }
            };

            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());
            _organizationDbSet.SetDbSetDataForAsync(organizations.AsQueryable());
            _kudosLogsDbSet.SetDbSetDataForAsync(kudosLogs.AsQueryable());
        }

        private void MockKudosLogsForStats()
        {
            // User1 has 274.4 Kudos
            // User2 has 20
            // USer3 has 33
            var kudosLogs = new List<KudosLog>
            {
                new()
                {
                    // +2
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    KudosSystemType = KudosTypeEnum.Ordinary,
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
                new()
                {
                    // +30
                    KudosTypeName = "Type1",
                    KudosTypeValue = 1,
                    KudosSystemType = KudosTypeEnum.Ordinary,
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
                new()
                {
                    // 0 (OrgID = 2)
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = KudosTypeEnum.Ordinary,
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
                new()
                {
                    // 0 (KudosType = Minus)
                    KudosTypeName = "Minus",
                    KudosTypeValue = 2,
                    KudosSystemType = KudosTypeEnum.Minus,
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
                new() // 0 (Status = Pending)
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = KudosTypeEnum.Ordinary,
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
                new() // +242.4
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = KudosTypeEnum.Ordinary,
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
                new() // +20
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = KudosTypeEnum.Ordinary,
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
                new() // 0 (OrgID = 1)
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = KudosTypeEnum.Ordinary,
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
                new() // +34
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = KudosTypeEnum.Ordinary,
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
                new() // 0 (DateTime out of range)
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = KudosTypeEnum.Ordinary,
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
                new() // +10
                {
                    KudosTypeName = "Type2",
                    KudosTypeValue = 2,
                    KudosSystemType = KudosTypeEnum.Ordinary,
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

            var users = new List<ApplicationUser>
            {
                new()
                {
                    Id = "User1",
                    FirstName = "User",
                    LastName = "1",
                    EmploymentDate = DateTime.UtcNow
                },
                new()
                {
                    Id = "User2",
                    FirstName = "User",
                    LastName = "2",
                    EmploymentDate = DateTime.UtcNow
                },
                new()
                {
                    Id = "User3",
                    FirstName = "User",
                    LastName = "3",
                    EmploymentDate = DateTime.UtcNow
                },
                new()
                {
                    Id = "User4",
                    FirstName = "User",
                    LastName = "4",
                    EmploymentDate = DateTime.UtcNow
                }
            };

            _usersDbSet.SetDbSetDataForAsync(users.AsQueryable());
            _kudosLogsDbSet.SetDbSetDataForAsync(kudosLogs.AsQueryable());
        }

        private void MockKudosLogsForProfileUpdate()
        {
            var kudosLogs = new List<KudosLog>
            {
                //should ingore pending
                new()
                {
                    Status = KudosStatus.Pending,
                    Id = 1,
                    EmployeeId = "Id",
                    KudosTypeName = "Type1",
                    KudosSystemType = KudosTypeEnum.Ordinary,
                    OrganizationId = 1,
                    Created = DateTime.UtcNow,
                    CreatedBy = "testUserId"
                },

                //should ingore organizationId
                new()
                {
                    Status = KudosStatus.Approved,
                    Id = 2,
                    EmployeeId = "Id",
                    KudosTypeName = "Type1",
                    KudosSystemType = KudosTypeEnum.Ordinary,
                    OrganizationId = 2,
                    Created = DateTime.UtcNow,
                    Points = 1
                },

                //Should ignore because of employment date
                new()
                {
                    Status = KudosStatus.Approved,
                    Id = 3,
                    EmployeeId = "Id",
                    KudosTypeName = "Type2",
                    KudosSystemType = KudosTypeEnum.Ordinary,
                    KudosTypeValue = 2,
                    OrganizationId = 1,
                    Created = DateTime.UtcNow.AddDays(-11),
                    Points = 1
                },
                new()
                {
                    Status = KudosStatus.Approved,
                    Id = 4,
                    EmployeeId = "Id",
                    KudosTypeName = "Type2",
                    KudosSystemType = KudosTypeEnum.Ordinary,
                    OrganizationId = 1,
                    Created = DateTime.UtcNow,
                    Points = 10
                },
                new()
                {
                    Status = KudosStatus.Approved,
                    Id = 5,
                    EmployeeId = "Id",
                    KudosTypeName = "Type2",
                    KudosSystemType = KudosTypeEnum.Ordinary,
                    OrganizationId = 1,
                    Created = DateTime.UtcNow,
                    Points = 1
                },
                new()
                {
                    Status = KudosStatus.Approved,
                    Id = 6,
                    EmployeeId = "Id",
                    KudosTypeName = "Minus",
                    KudosSystemType = KudosTypeEnum.Minus,
                    OrganizationId = 1,
                    Created = DateTime.UtcNow,
                    Points = 1
                },
                new()
                {
                    Status = KudosStatus.Approved,
                    Id = 6,
                    EmployeeId = "Id",
                    KudosTypeName = "Other",
                    KudosSystemType = KudosTypeEnum.Other,
                    OrganizationId = 1,
                    Created = DateTime.UtcNow,
                    Points = 1,
                    KudosBasketId = 1
                }
            };
            _kudosLogsDbSet.SetDbSetDataForAsync(kudosLogs.AsQueryable());
        }

        #endregion
    }
}
