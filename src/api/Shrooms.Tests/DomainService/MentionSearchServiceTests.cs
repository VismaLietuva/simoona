using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Wall.Mentions;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Group;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.Wall.Mentions;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class MentionSearchServiceTests
    {
        private const int OrganizationId = 1;
        private const string JaneId = "9f3e4c7a-1b2d-4e5f-8a9b-0c1d2e3f4a5b";

        private static readonly DateTime Now = new DateTime(2026, 8, 24, 10, 0, 0, DateTimeKind.Utc);

        private DbSet<Group> _groupsDbSet;
        private IMentionSearchService _searchService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            uow.MockDbSetForAsync<ApplicationUser>();
            _groupsDbSet = uow.MockDbSetForAsync<Group>();

            var roleService = Substitute.For<IRoleService>();
            roleService.ExcludeUsersWithRole(Arg.Any<string>())
                .Returns((Expression<Func<ApplicationUser, bool>>)(user => true));

            var systemClock = Substitute.For<ISystemClock>();
            systemClock.UtcNow.Returns(Now);

            _searchService = new MentionSearchService(uow, roleService, systemClock);
        }

        [Test]
        public async Task Should_List_Groups_With_Members_Before_Empty_Ones()
        {
            GivenGroups(
                TaggableGroup(1, "Alpha"),
                TaggableGroup(2, "Beta", JaneId),
                TaggableGroup(3, "Gamma"));

            var groups = await SearchGroupsAsync(string.Empty);

            Assert.That(
                groups.Select(group => group.Name),
                Is.EqualTo(new[] { "Beta", "Alpha", "Gamma" }));
        }

        [Test]
        public async Task Should_Not_Let_Empty_Groups_Push_A_Pickable_One_Past_The_Limit()
        {
            GivenGroups(
                TaggableGroup(1, "Team A"),
                TaggableGroup(2, "Team B"),
                TaggableGroup(3, "Team C"),
                TaggableGroup(4, "Team D"),
                TaggableGroup(5, "Team E"),
                TaggableGroup(6, "Team F", JaneId));

            var groups = await SearchGroupsAsync("Team");

            Assert.That(groups, Has.Length.EqualTo(5));
            Assert.That(groups.First().Name, Is.EqualTo("Team F"));
            Assert.That(groups.First().MemberCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Rank_A_Name_Match_First_Among_Groups_With_Members()
        {
            GivenGroups(
                TaggableGroup(1, "Product Marketing", JaneId),
                TaggableGroup(2, "Marketing", JaneId));

            var groups = await SearchGroupsAsync("Marketing");

            Assert.That(
                groups.Select(group => group.Name),
                Is.EqualTo(new[] { "Marketing", "Product Marketing" }));
        }

        [Test]
        public async Task Should_Count_Only_Members_Active_Today()
        {
            var group = TaggableGroup(1, "Marketing", JaneId);
            group.Members.Add(new GroupMember { UserId = "left-last-week", EndDate = Now.AddDays(-7) });
            GivenGroups(group);

            var groups = await SearchGroupsAsync("Marketing");

            Assert.That(groups.Single().MemberCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Still_List_A_Finished_Group()
        {
            var group = TaggableGroup(1, "Task Force", JaneId);
            group.EndDate = Now.AddDays(-30);
            GivenGroups(group);

            var groups = await SearchGroupsAsync("Task");

            Assert.That(groups.Single().Name, Is.EqualTo("Task Force"));
        }

        [Test]
        public async Task Should_Skip_A_Group_Whose_Type_Is_Not_Taggable()
        {
            var group = TaggableGroup(1, "Marketing", JaneId);
            group.GroupType.HasGroupTag = false;
            GivenGroups(group);

            var groups = await SearchGroupsAsync("Marketing");

            Assert.That(groups, Is.Empty);
        }

        private async Task<MentionGroupDto[]> SearchGroupsAsync(string term)
        {
            var suggestions = await _searchService.SearchAsync(
                term,
                new UserAndOrganizationDto { OrganizationId = OrganizationId });

            return suggestions.Groups.ToArray();
        }

        private void GivenGroups(params Group[] groups)
        {
            _groupsDbSet.SetDbSetDataForAsync(groups.ToList());
        }

        private static Group TaggableGroup(int id, string name, params string[] memberIds)
        {
            return new Group
            {
                Id = id,
                Name = name,
                OrganizationId = OrganizationId,
                Status = GroupStatus.Approved,
                GroupType = new GroupType { Id = id, HasGroupTag = true },
                Members = memberIds
                    .Select(memberId => new GroupMember { UserId = memberId })
                    .ToList()
            };
        }
    }
}
