using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Search;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.Search;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class GlobalSearchServiceTests
    {
        private const int OrganizationId = 1;
        private const int OtherOrganizationId = 2;
        private const string UserId = "9f3e4c7a-1b2d-4e5f-8a9b-0c1d2e3f4a5b";

        private static readonly DateTime Now = new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc);

        private static readonly string[] AllPermissions =
        {
            BasicPermissions.EmployeeList,
            BasicPermissions.Post,
            BasicPermissions.Event
        };

        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<Wall> _wallsDbSet;
        private DbSet<Event> _eventsDbSet;
        private IPermissionService _permissionService;
        private IGlobalSearchService _searchService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = uow.MockDbSetForAsync<ApplicationUser>();
            _wallsDbSet = uow.MockDbSetForAsync<Wall>();
            _eventsDbSet = uow.MockDbSetForAsync<Event>();

            _permissionService = Substitute.For<IPermissionService>();
            GivenPermissions(AllPermissions);

            var roleService = Substitute.For<IRoleService>();
            roleService.ExcludeUsersWithRole(Arg.Any<string>())
                .Returns((Expression<Func<ApplicationUser, bool>>)(user => true));

            var systemClock = Substitute.For<ISystemClock>();
            systemClock.UtcNow.Returns(Now);

            _searchService = new GlobalSearchService(uow, _permissionService, roleService, systemClock);
        }

        [Test]
        public void Should_Stop_The_Search_When_The_Token_Is_Cancelled()
        {
            Assert.That(
                async () => await _searchService.SearchAsync(
                    "Kudos",
                    8,
                    CurrentUser(),
                    new CancellationToken(true)),
                Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public async Task Should_Return_Matching_People_Walls_And_Events()
        {
            GivenUsers(Person("u1", "Tomas", "Kudosas", "QA engineer"), Person("u2", "Ona", "Petraite", "Developer"));
            GivenWalls(UserWall(1, "Kudos shoutouts", "m1", "m2"), UserWall(2, "Vilnius office"));
            GivenEvents(EventAt("Kudos evening", "Vilnius", Now.AddDays(5)), EventAt("Guild meetup", "Online", Now.AddDays(6)));

            var results = await SearchAsync("Kudos");

            Assert.That(results.People.Select(person => person.FullName), Is.EqualTo(new[] { "Tomas Kudosas" }));
            Assert.That(results.People.Single().JobTitle, Is.EqualTo("QA engineer"));
            Assert.That(results.Walls.Select(wall => wall.Name), Is.EqualTo(new[] { "Kudos shoutouts" }));
            Assert.That(results.Walls.Single().TotalMembers, Is.EqualTo(2));
            Assert.That(results.Events.Select(e => e.Name), Is.EqualTo(new[] { "Kudos evening" }));
        }

        [Test]
        public async Task Should_Return_Nothing_And_Skip_The_Database_For_A_Search_That_Is_Too_Short()
        {
            GivenWalls(UserWall(1, "Kudos shoutouts"));

            var results = await SearchAsync(" K ");

            Assert.That(results.People, Is.Empty);
            Assert.That(results.Walls, Is.Empty);
            Assert.That(results.Events, Is.Empty);
            await _permissionService.DidNotReceiveWithAnyArgs().GetUserPermissionsAsync(default, default, default);
        }

        [Test]
        public async Task Should_Leave_A_Group_Empty_When_The_User_Has_No_Permission_For_It()
        {
            GivenPermissions(BasicPermissions.Post);
            GivenUsers(Person("u1", "Tomas", "Kudosas", "QA engineer"));
            GivenWalls(UserWall(1, "Kudos shoutouts"));
            GivenEvents(EventAt("Kudos evening", "Vilnius", Now.AddDays(5)));

            var results = await SearchAsync("Kudos");

            Assert.That(results.People, Is.Empty);
            Assert.That(results.Events, Is.Empty);
            Assert.That(results.Walls.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Not_Return_Anything_From_Another_Organization()
        {
            var foreignPerson = Person("u1", "Tomas", "Kudosas", "QA engineer");
            foreignPerson.OrganizationId = OtherOrganizationId;
            var foreignWall = UserWall(1, "Kudos shoutouts");
            foreignWall.OrganizationId = OtherOrganizationId;
            var foreignEvent = EventAt("Kudos evening", "Vilnius", Now.AddDays(5));
            foreignEvent.OrganizationId = OtherOrganizationId;
            GivenUsers(foreignPerson);
            GivenWalls(foreignWall);
            GivenEvents(foreignEvent);

            var results = await SearchAsync("Kudos");

            Assert.That(results.People, Is.Empty);
            Assert.That(results.Walls, Is.Empty);
            Assert.That(results.Events, Is.Empty);
        }

        [Test]
        public async Task Should_Cap_Each_Group_At_The_Limit()
        {
            GivenWalls(UserWall(1, "Kudos A"), UserWall(2, "Kudos B"), UserWall(3, "Kudos C"));

            var results = await _searchService.SearchAsync("Kudos", 2, CurrentUser());

            Assert.That(results.Walls.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Match_A_Person_On_Every_Word_Across_Name_And_Job_Title()
        {
            GivenUsers(
                Person("u1", "Tomas", "Kudosas", "QA engineer"),
                Person("u2", "Tomas", "Jonaitis", "Developer"));

            var results = await SearchAsync("Tomas QA");

            Assert.That(results.People.Select(person => person.Id), Is.EqualTo(new[] { "u1" }));
        }

        [Test]
        public async Task Should_List_People_Whose_Full_Name_Starts_With_The_Search_First()
        {
            GivenUsers(
                Person("u1", "Adam", "Tomason", "Developer"),
                Person("u2", "Tomas", "Kudosas", "QA engineer"));

            var results = await SearchAsync("Toma");

            Assert.That(results.People.Select(person => person.Id), Is.EqualTo(new[] { "u2", "u1" }));
        }

        [Test]
        public async Task Should_List_Walls_Starting_With_The_Search_First()
        {
            GivenWalls(UserWall(1, "Team kudos"), UserWall(2, "kudos shoutouts"));

            var results = await SearchAsync("kudos");

            Assert.That(results.Walls.Select(wall => wall.Id), Is.EqualTo(new[] { 2, 1 }));
        }

        [Test]
        public async Task Should_Not_Offer_Event_Walls()
        {
            var eventWall = UserWall(1, "Kudos evening");
            eventWall.Type = WallType.Events;
            GivenWalls(eventWall);

            var results = await SearchAsync("Kudos");

            Assert.That(results.Walls, Is.Empty);
        }

        [Test]
        public async Task Should_List_Upcoming_Events_Before_Finished_Ones()
        {
            GivenEvents(
                EventAt("Kudos 2025", "Vilnius", Now.AddDays(-300)),
                EventAt("Kudos evening", "Vilnius", Now.AddDays(5)),
                EventAt("Kudos last month", "Vilnius", Now.AddDays(-30)));

            var results = await SearchAsync("Kudos");

            Assert.That(
                results.Events.Select(e => e.Name),
                Is.EqualTo(new[] { "Kudos evening", "Kudos last month", "Kudos 2025" }));
        }

        [Test]
        public async Task Should_Find_An_Event_By_Its_Place()
        {
            GivenEvents(EventAt("Guild meetup", "Kaunas office", Now.AddDays(5)));

            var results = await SearchAsync("Kaunas");

            Assert.That(results.Events.Single().Name, Is.EqualTo("Guild meetup"));
        }

        [Test]
        public async Task Should_Return_When_An_Event_Starts_And_Ends()
        {
            var start = Now.AddDays(5);
            GivenEvents(EventAt("Kudos evening", "Vilnius", start));

            var found = (await SearchAsync("Kudos")).Events.Single();

            Assert.That(found.StartDate, Is.EqualTo(start));
            Assert.That(found.EndDate, Is.EqualTo(start.AddHours(2)));
        }

        private Task<GlobalSearchResultsDto> SearchAsync(string search)
        {
            return _searchService.SearchAsync(search, 8, CurrentUser());
        }

        private static UserAndOrganizationDto CurrentUser()
        {
            return new UserAndOrganizationDto { OrganizationId = OrganizationId, UserId = UserId };
        }

        private void GivenPermissions(params string[] permissions)
        {
            _permissionService
                .GetUserPermissionsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(permissions);
        }

        private void GivenUsers(params ApplicationUser[] users)
        {
            _usersDbSet.SetDbSetDataForAsync(users.ToList());
        }

        private void GivenWalls(params Wall[] walls)
        {
            _wallsDbSet.SetDbSetDataForAsync(walls.ToList());
        }

        private void GivenEvents(params Event[] events)
        {
            _eventsDbSet.SetDbSetDataForAsync(events.ToList());
        }

        private static ApplicationUser Person(string id, string firstName, string lastName, string jobTitle)
        {
            return new ApplicationUser
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                OrganizationId = OrganizationId,
                JobPosition = new JobPosition { Title = jobTitle }
            };
        }

        private static Wall UserWall(int id, string name, params string[] memberIds)
        {
            return new Wall
            {
                Id = id,
                Name = name,
                OrganizationId = OrganizationId,
                Type = WallType.UserCreated,
                Members = memberIds
                    .Select(memberId => new WallMember { UserId = memberId })
                    .ToList()
            };
        }

        private static Event EventAt(string name, string place, DateTime startDate)
        {
            return new Event
            {
                Id = Guid.NewGuid(),
                Name = name,
                Place = place,
                OrganizationId = OrganizationId,
                StartDate = startDate,
                EndDate = startDate.AddHours(2)
            };
        }
    }
}
