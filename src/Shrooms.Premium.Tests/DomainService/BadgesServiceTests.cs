using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Badges;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.Domain.Services.Badges;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.UnitTests.DomainService
{
    [TestFixture]
    public class BadgesServiceTests
    {
        private BadgesService _badgesService;

        private IUnitOfWork2 _unitOfWork;

        private IDbSet<BadgeCategory> _badgeCategoriesDbSet;
        private IDbSet<BadgeLog> _badgeLogsDbSet;
        private IDbSet<KudosLog> _kudosLogsDbSet;
        private IDbSet<ApplicationUser> _usersDbSet;

        [SetUp]
        public void TestInitializer()
        {
            _unitOfWork = Substitute.For<IUnitOfWork2>();

            _badgeCategoriesDbSet = _unitOfWork.MockDbSet<BadgeCategory>();
            _badgeLogsDbSet = _unitOfWork.MockDbSet<BadgeLog>();
            _kudosLogsDbSet = _unitOfWork.MockDbSet<KudosLog>();
            _usersDbSet = _unitOfWork.MockDbSet<ApplicationUser>();

            _badgesService = new BadgesService(_unitOfWork);
        }

        [Test]
        public async Task Should_Assign_1_Badge_For_Recent_Kudos_Transaction()
        {
            // Arrange
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            var user = new ApplicationUser { Id = "user1", OrganizationId = 1, EmploymentDate = twoDaysAgo, BadgeLogs = new List<BadgeLog>() };
            _badgeCategoriesDbSet.SetDbSetDataForAsync(GetBadgeCategories(twoDaysAgo));
            _kudosLogsDbSet.SetDbSetDataForAsync(GetKudosLogs(user, oneHourAgo));
            _badgeLogsDbSet.SetDbSetDataForAsync(new List<BadgeLog>());

            _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

            // Act
            await _badgesService.AssignBadgesAsync();

            // Assert
            AssertBadgeReceived(user.Id, 1, 1);
        }

        [Test]
        public async Task Should_Assign_1_Badge_For_Recent_New_Badge_Category()
        {
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            var user = new ApplicationUser { Id = "user1", OrganizationId = 1, EmploymentDate = twoDaysAgo, BadgeLogs = new List<BadgeLog>(), TotalKudos = 200 };
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { user });
            _badgeCategoriesDbSet.SetDbSetDataForAsync(GetBadgeCategories(oneHourAgo, twoDaysAgo));
            _kudosLogsDbSet.SetDbSetDataForAsync(GetKudosLogs(user, twoDaysAgo));
            _badgeLogsDbSet.SetDbSetDataForAsync(new List<BadgeLog>());

            _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

            // Act
            await _badgesService.AssignBadgesAsync();

            // Assert
            AssertBadgeReceived(user.Id, 1, 1);
        }

        [Test]
        public async Task Should_Assign_1_Badge_For_Recent_New_Badge_Type()
        {
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            var user = new ApplicationUser { Id = "user1", OrganizationId = 1, EmploymentDate = twoDaysAgo, BadgeLogs = new List<BadgeLog>(), TotalKudos = 200 };
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { user });
            _badgeCategoriesDbSet.SetDbSetDataForAsync(GetBadgeCategories(twoDaysAgo, oneHourAgo));
            _kudosLogsDbSet.SetDbSetDataForAsync(GetKudosLogs(user, twoDaysAgo));
            _badgeLogsDbSet.SetDbSetDataForAsync(new List<BadgeLog>());

            _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

            // Act
            await _badgesService.AssignBadgesAsync();

            // Assert
            AssertBadgeReceived(user.Id, 1, 1);
        }

        public async Task Should_Not_Assign_Badge_For_No_Changes_Or_New_Kudos()
        {
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);
            var user = new ApplicationUser { Id = "user1", OrganizationId = 1, EmploymentDate = twoDaysAgo, BadgeLogs = new List<BadgeLog>(), TotalKudos = 200 };
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser> { user });
            _badgeCategoriesDbSet.SetDbSetDataForAsync(GetBadgeCategories(twoDaysAgo));
            _kudosLogsDbSet.SetDbSetDataForAsync(GetKudosLogs(user, twoDaysAgo));
            _badgeLogsDbSet.SetDbSetDataForAsync(new List<BadgeLog>());

            _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(0));

            // Act
            await _badgesService.AssignBadgesAsync();

            // Assert
            _badgeLogsDbSet.DidNotReceive().Add(Arg.Is<BadgeLog>(x => x.EmployeeId == user.Id && x.BadgeTypeId == 1 && x.OrganizationId == 1));
        }

        private IEnumerable<BadgeCategory> GetBadgeCategories(DateTime someTimeAgoForCategories, DateTime? someTimeAgoForTypes = null)
        {
            if (!someTimeAgoForTypes.HasValue)
            {
                someTimeAgoForTypes = someTimeAgoForCategories;
            }

            return new List<BadgeCategory>
            {
                new BadgeCategory
                {
                    Id = 1,
                    Created = someTimeAgoForCategories,
                    Description = "Choco desc",
                    Title = "Choco",
                    BadgeTypes = new List<BadgeType>
                    {
                        new BadgeType { Id = 1, BadgeCategoryId = 1, Value = 15, Created = someTimeAgoForTypes.Value, Title = "Cookie", IsActive = true },
                        new BadgeType { Id = 1, BadgeCategoryId = 1, Value = 16, Created = someTimeAgoForTypes.Value, Title = "Cookie monster", IsActive = true }
                    },
                    RelationshipsWithKudosTypes = new List<BadgeCategoryKudosType>
                    {
                        new BadgeCategoryKudosType
                        {
                            KudosType = new KudosType { Name = "Choco" },
                            CalculationPolicyType = BadgeCalculationPolicyType.PointsStrategy
                        },
                        new BadgeCategoryKudosType
                        {
                            KudosType = new KudosType { Name = "Choco two kudos type" },
                            CalculationPolicyType = BadgeCalculationPolicyType.MultiplierStrategy
                        }
                    }
                },
                new BadgeCategory
                {
                    Id = 2,
                    Created = someTimeAgoForCategories,
                    Description = "Ment desc",
                    Title = "Ment",
                    BadgeTypes = new List<BadgeType>
                    {
                        new BadgeType { BadgeCategoryId = 2, Created = someTimeAgoForTypes.Value, Title = "Rookie mentor", IsActive = true }
                    },
                    RelationshipsWithKudosTypes = new List<BadgeCategoryKudosType>
                    {
                        new BadgeCategoryKudosType
                        {
                            KudosType = new KudosType { Name = "Ment" },
                            CalculationPolicyType = BadgeCalculationPolicyType.MultiplierStrategy
                        }
                    }
                }
            };
        }

        private IEnumerable<KudosLog> GetKudosLogs(ApplicationUser user, DateTime someTimeAgo)
        {
            return new List<KudosLog>
            {
                new KudosLog
                {
                    EmployeeId = user.Id,
                    Employee = user,
                    Points = 10,
                    MultiplyBy = 2,
                    KudosTypeName = "Choco",
                    Status = KudosStatus.Approved,
                    Modified = someTimeAgo,
                    KudosSystemType = KudosTypeEnum.Ordinary
                },
                new KudosLog
                {
                    EmployeeId = user.Id,
                    Employee = user,
                    Points = 6,
                    MultiplyBy = 5,
                    KudosTypeName = "Choco two kudos type",
                    Status = KudosStatus.Approved,
                    Modified = someTimeAgo,
                    KudosSystemType = KudosTypeEnum.Ordinary
                }
            };
        }

        private void AssertBadgeReceived(string userId, int badgeTypeId, int organizationId)
        {
            _badgeLogsDbSet.Received().Add(Arg.Is<BadgeLog>(x => x.EmployeeId == userId && x.BadgeTypeId == badgeTypeId && x.OrganizationId == organizationId));
            Received.InOrder(async () => await _unitOfWork.SaveChangesAsync());
        }
    }
}
