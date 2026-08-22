using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Group;
using Shrooms.Domain.Services.Wall.Mentions;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class MentionResolverTests
    {
        private const int OrganizationId = 1;
        private const string AuthorId = "aaaaaaaa-1111-2222-3333-444444444444";
        private const string JaneId = "9f3e4c7a-1b2d-4e5f-8a9b-0c1d2e3f4a5b";
        private const string JohnId = "11112222-3333-4444-5555-666677778888";

        private static readonly DateTime Now = new DateTime(2026, 8, 22, 10, 0, 0, DateTimeKind.Utc);

        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<Group> _groupsDbSet;
        private IMentionResolver _resolver;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = uow.MockDbSetForAsync<ApplicationUser>();
            _groupsDbSet = uow.MockDbSetForAsync<Group>();

            var systemClock = Substitute.For<ISystemClock>();
            systemClock.UtcNow.Returns(Now);

            _resolver = new MentionResolver(uow, systemClock);
        }

        [Test]
        public async Task Should_Resolve_A_Mentioned_User()
        {
            GivenUsers(JaneId, JohnId, AuthorId);

            var resolved = await ResolveAsync($"hi @[Jane Doe](user:{JaneId})");

            Assert.That(resolved, Is.EquivalentTo(new[] { JaneId }));
        }

        [Test]
        public async Task Should_Ignore_A_User_From_Another_Organization()
        {
            _usersDbSet.SetDbSetDataForAsync(new List<ApplicationUser>
            {
                new ApplicationUser { Id = AuthorId, OrganizationId = OrganizationId },
                new ApplicationUser { Id = JaneId, OrganizationId = 99 }
            });

            var resolved = await ResolveAsync($"@[Jane Doe](user:{JaneId})");

            Assert.That(resolved, Is.Empty);
        }

        [Test]
        public async Task Should_Not_Notify_The_Author_About_Their_Own_Mention()
        {
            GivenUsers(AuthorId, JaneId);

            var resolved = await ResolveAsync($"@[Me](user:{AuthorId}) @[Jane Doe](user:{JaneId})");

            Assert.That(resolved, Is.EquivalentTo(new[] { JaneId }));
        }

        [Test]
        public async Task Should_Ignore_A_Malformed_Token()
        {
            GivenUsers(JaneId);

            var resolved = await ResolveAsync("@[Jane Doe](user:not-a-guid)");

            Assert.That(resolved, Is.Empty);
        }

        [Test]
        public async Task Should_Expand_A_Tagged_Group_To_Its_Active_Members()
        {
            GivenGroups(TaggableGroup(12, JaneId, JohnId));

            var resolved = await ResolveAsync("@[Marketing](group:12)");

            Assert.That(resolved, Is.EquivalentTo(new[] { JaneId, JohnId }));
        }

        [Test]
        public async Task Should_Not_Expand_A_Group_Whose_Type_Is_Not_Taggable()
        {
            var group = TaggableGroup(12, JaneId);
            group.GroupType.HasGroupTag = false;
            GivenGroups(group);

            var resolved = await ResolveAsync("@[Marketing](group:12)");

            Assert.That(resolved, Is.Empty);
        }

        [Test]
        public async Task Should_Not_Expand_A_Group_Awaiting_Approval()
        {
            var group = TaggableGroup(12, JaneId);
            group.Status = GroupStatus.Pending;
            GivenGroups(group);

            var resolved = await ResolveAsync("@[Marketing](group:12)");

            Assert.That(resolved, Is.Empty);
        }

        [Test]
        public async Task Should_Still_Expand_A_Group_On_Its_Final_Day()
        {
            var group = TaggableGroup(12, JaneId);
            group.EndDate = Now.Date;
            GivenGroups(group);

            var resolved = await ResolveAsync("@[Task Force](group:12)");

            Assert.That(resolved, Is.EquivalentTo(new[] { JaneId }));
        }

        [Test]
        public async Task Should_Not_Expand_An_Expired_Group()
        {
            var group = TaggableGroup(12, JaneId);
            group.EndDate = Now.AddDays(-1);
            GivenGroups(group);

            var resolved = await ResolveAsync("@[Task Force](group:12)");

            Assert.That(resolved, Is.Empty);
        }

        [Test]
        public async Task Should_Skip_A_Member_Whose_Membership_Has_Ended()
        {
            var group = TaggableGroup(12, JaneId);
            group.Members.Add(new GroupMember
            {
                UserId = JohnId,
                StartDate = Now.AddYears(-2),
                EndDate = Now.AddDays(-3)
            });
            GivenGroups(group);

            var resolved = await ResolveAsync("@[Marketing](group:12)");

            Assert.That(resolved, Is.EquivalentTo(new[] { JaneId }));
        }

        [Test]
        public async Task Should_Skip_A_Member_Whose_Membership_Has_Not_Started()
        {
            var group = TaggableGroup(12, JaneId);
            group.Members.Add(new GroupMember { UserId = JohnId, StartDate = Now.AddDays(3) });
            GivenGroups(group);

            var resolved = await ResolveAsync("@[Marketing](group:12)");

            Assert.That(resolved, Is.EquivalentTo(new[] { JaneId }));
        }

        [Test]
        public async Task Should_Notify_A_Person_In_Two_Tagged_Groups_Once()
        {
            GivenGroups(TaggableGroup(12, JaneId), TaggableGroup(13, JaneId, JohnId));

            var resolved = await ResolveAsync("@[Marketing](group:12) @[Guild](group:13)");

            Assert.That(resolved, Is.EquivalentTo(new[] { JaneId, JohnId }));
        }

        [Test]
        public async Task Should_Not_Notify_The_Author_Through_A_Group_They_Belong_To()
        {
            GivenGroups(TaggableGroup(12, AuthorId, JaneId));

            var resolved = await ResolveAsync("@[Marketing](group:12)");

            Assert.That(resolved, Is.EquivalentTo(new[] { JaneId }));
        }

        [Test]
        public async Task Should_Combine_A_Person_And_A_Group_Tag()
        {
            GivenUsers(JaneId);
            GivenGroups(TaggableGroup(12, JohnId));

            var resolved = await ResolveAsync($"@[Jane Doe](user:{JaneId}) @[Marketing](group:12)");

            Assert.That(resolved, Is.EquivalentTo(new[] { JaneId, JohnId }));
        }

        [Test]
        public async Task Should_Fall_Back_To_Client_Sent_Ids_When_The_Body_Has_No_Tokens()
        {
            GivenUsers(JaneId);

            var resolved = await ResolveAsync("hello @Jane_Doe", new[] { JaneId });

            Assert.That(resolved, Is.EquivalentTo(new[] { JaneId }));
        }

        [Test]
        public async Task Should_Ignore_Client_Sent_Ids_When_The_Body_Has_Tokens()
        {
            GivenUsers(JaneId, JohnId);

            var resolved = await ResolveAsync($"@[Jane Doe](user:{JaneId})", new[] { JohnId });

            Assert.That(resolved, Is.EquivalentTo(new[] { JaneId }));
        }

        [Test]
        public async Task Should_Resolve_Nothing_For_An_Empty_Body()
        {
            var resolved = await ResolveAsync(null);

            Assert.That(resolved, Is.Empty);
        }

        [Test]
        public async Task Should_Not_Notify_Again_Someone_The_Previous_Body_Already_Mentioned()
        {
            GivenUsers(JaneId, JohnId);

            var resolved = await ResolveAddedAsync(
                $"@[Jane Doe](user:{JaneId}) @[John Smith](user:{JohnId}) typo fixed",
                $"@[Jane Doe](user:{JaneId})");

            Assert.That(resolved, Is.EquivalentTo(new[] { JohnId }));
        }

        [Test]
        public async Task Should_Notify_Nobody_When_An_Edit_Adds_No_Mentions()
        {
            GivenUsers(JaneId);

            var resolved = await ResolveAddedAsync(
                $"@[Jane Doe](user:{JaneId}) typo fixed",
                $"@[Jane Doe](user:{JaneId})");

            Assert.That(resolved, Is.Empty);
        }

        [Test]
        public async Task Should_Match_A_Previous_Mention_Regardless_Of_Guid_Casing()
        {
            GivenUsers(JaneId);

            var resolved = await ResolveAddedAsync(
                $"@[Jane Doe](user:{JaneId.ToUpperInvariant()})",
                $"@[Jane Doe](user:{JaneId})");

            Assert.That(resolved, Is.Empty);
        }

        [Test]
        public async Task Should_Not_Expand_A_Group_The_Previous_Body_Already_Tagged()
        {
            GivenGroups(TaggableGroup(12, JaneId, JohnId));

            var resolved = await ResolveAddedAsync(
                "@[Marketing](group:12) typo fixed",
                "@[Marketing](group:12)");

            Assert.That(resolved, Is.Empty);
        }

        [Test]
        public async Task Should_Ignore_Tokens_Past_The_Per_Message_Cap()
        {
            var ids = Enumerable.Range(0, 25)
                .Select(index => $"00000000-0000-0000-0000-{index:D12}")
                .ToArray();

            GivenUsers(ids);

            var body = string.Concat(ids.Select(id => $"@[Someone](user:{id}) "));

            var resolved = await ResolveAsync(body);

            Assert.That(resolved.Count(), Is.EqualTo(20));
        }

        private Task<IEnumerable<string>> ResolveAsync(string messageBody, IEnumerable<string> legacyUserIds = null)
        {
            return _resolver.ResolveAsync(
                messageBody,
                legacyUserIds,
                new UserAndOrganizationDto { UserId = AuthorId, OrganizationId = OrganizationId });
        }

        private Task<IEnumerable<string>> ResolveAddedAsync(string messageBody, string previousMessageBody)
        {
            return _resolver.ResolveAddedAsync(
                messageBody,
                previousMessageBody,
                null,
                new UserAndOrganizationDto { UserId = AuthorId, OrganizationId = OrganizationId });
        }

        private void GivenUsers(params string[] ids)
        {
            _usersDbSet.SetDbSetDataForAsync(ids
                .Select(id => new ApplicationUser { Id = id, OrganizationId = OrganizationId })
                .ToList());
        }

        private void GivenGroups(params Group[] groups)
        {
            _groupsDbSet.SetDbSetDataForAsync(groups.ToList());
        }

        private static Group TaggableGroup(int id, params string[] memberIds)
        {
            return new Group
            {
                Id = id,
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
