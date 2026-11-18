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
using Shrooms.Contracts.DataTransferObjects.Users;
using Shrooms.Contracts.DataTransferObjects.Wall.Comments;
using Shrooms.Contracts.DataTransferObjects.Wall.Posts;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.DataLayer.EntityModels.Models.Polls;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Domain.Services.Roles;
using Shrooms.Domain.Services.Search;
using Shrooms.Domain.Services.Wall;
using Shrooms.Tests.Extensions;
using GroupEntity = Shrooms.DataLayer.EntityModels.Models.Group.Group;

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
            BasicPermissions.Event,
            BasicPermissions.Kudos,
            BasicPermissions.Project,
            BasicPermissions.Groups,
            BasicPermissions.Book,
            BasicPermissions.Poll,
            BasicPermissions.VideoLibrary,
            BasicPermissions.ServiceRequest
        };

        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<Wall> _wallsDbSet;
        private DbSet<Event> _eventsDbSet;
        private DbSet<KudosLog> _kudosLogsDbSet;
        private DbSet<Project> _projectsDbSet;
        private DbSet<GroupEntity> _groupsDbSet;
        private DbSet<BookOffice> _bookOfficesDbSet;
        private DbSet<Poll> _pollsDbSet;
        private DbSet<VideoLibraryItem> _videosDbSet;
        private DbSet<ServiceRequest> _serviceRequestsDbSet;
        private DbSet<ServiceRequestCategory> _serviceRequestCategoriesDbSet;
        private IPermissionService _permissionService;
        private IWallService _wallService;
        private IGlobalSearchService _searchService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = uow.MockDbSetForAsync<ApplicationUser>();
            _wallsDbSet = uow.MockDbSetForAsync<Wall>();
            _eventsDbSet = uow.MockDbSetForAsync<Event>();
            _kudosLogsDbSet = uow.MockDbSetForAsync<KudosLog>();
            _projectsDbSet = uow.MockDbSetForAsync<Project>();
            _groupsDbSet = uow.MockDbSetForAsync<GroupEntity>();
            _bookOfficesDbSet = uow.MockDbSetForAsync<BookOffice>();
            _pollsDbSet = uow.MockDbSetForAsync<Poll>();
            _videosDbSet = uow.MockDbSetForAsync<VideoLibraryItem>();
            _serviceRequestsDbSet = uow.MockDbSetForAsync<ServiceRequest>();
            _serviceRequestCategoriesDbSet = uow.MockDbSetForAsync<ServiceRequestCategory>();

            _permissionService = Substitute.For<IPermissionService>();
            GivenPermissions(AllPermissions);

            var roleService = Substitute.For<IRoleService>();
            roleService.ExcludeUsersWithRole(Arg.Any<string>())
                .Returns((Expression<Func<ApplicationUser, bool>>)(user => true));

            var systemClock = Substitute.For<ISystemClock>();
            systemClock.UtcNow.Returns(Now);

            _wallService = Substitute.For<IWallService>();
            GivenPosts();

            _searchService = new GlobalSearchService(uow, _permissionService, roleService, systemClock, _wallService);
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

        [Test]
        public async Task Should_Return_Posts_From_The_Wall_Search()
        {
            GivenPosts(new PostDto
            {
                Id = 7,
                MessageBody = "Huge Kudos to the platform team",
                Author = new UserDto { FullName = "Ona Petraite" },
                WallName = "Vilnius office",
                Created = Now.AddDays(-2)
            });

            var post = (await SearchAsync("kudos")).Posts.Single();

            Assert.That(post.Id, Is.EqualTo(7));
            Assert.That(post.Text, Is.EqualTo("Huge Kudos to the platform team"));
            Assert.That(post.AuthorFullName, Is.EqualTo("Ona Petraite"));
            Assert.That(post.WallName, Is.EqualTo("Vilnius office"));
            await _wallService.Received(1).SearchWallAsync("kudos", Arg.Any<UserAndOrganizationDto>(), 1, 8, Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task Should_Show_The_Comment_When_A_Post_Was_Found_Through_It()
        {
            GivenPosts(new PostDto
            {
                Id = 7,
                MessageBody = "Release is out",
                Comments = new[]
                {
                    new CommentDto { MessageBody = "Nice work" },
                    new CommentDto { MessageBody = "Kudos to everyone", IsHidden = true },
                    new CommentDto { MessageBody = "Well deserved kudos" }
                }
            });

            var post = (await SearchAsync("kudos")).Posts.Single();

            Assert.That(post.Text, Is.EqualTo("Well deserved kudos"));
        }

        [Test]
        public async Task Should_Not_Leak_The_Body_Of_A_Hidden_Post()
        {
            GivenPosts(new PostDto
            {
                Id = 7,
                MessageBody = "Hidden kudos rant",
                IsHidden = true,
                Comments = new[] { new CommentDto { MessageBody = "kudos anyway" } }
            });

            var post = (await SearchAsync("kudos")).Posts.Single();

            Assert.That(post.Text, Is.EqualTo("kudos anyway"));
        }

        [Test]
        public async Task Should_Not_Search_Posts_Without_The_Post_Permission()
        {
            GivenPermissions(BasicPermissions.Event);
            GivenPosts(new PostDto { Id = 7, MessageBody = "Kudos" });

            var results = await SearchAsync("Kudos");

            Assert.That(results.Posts, Is.Empty);
            await _wallService.DidNotReceiveWithAnyArgs().SearchWallAsync(default, default, default, default, default);
        }

        [Test]
        public async Task Should_Find_Kudos_By_Its_Comment()
        {
            GivenUsers(Person("sender", "Ona", "Petraite", "Developer"));
            GivenKudos(Kudos(1, "sender", "Tomas", "Kudosas", "Teamwork", "Great release"));

            var found = (await SearchAsync("release")).Kudos.Single();

            Assert.That(found.Title, Is.EqualTo("Ona Petraite → Tomas Kudosas"));
            Assert.That(found.Description, Is.EqualTo("Teamwork · Great release"));
            Assert.That(found.LinkId, Is.EqualTo("receiver-1"));
        }

        [Test]
        public async Task Should_Not_Find_Kudos_By_Sender_Receiver_Or_Type()
        {
            GivenUsers(Person("sender", "Ona", "Petraite", "Developer"));
            GivenKudos(Kudos(1, "sender", "Tomas", "Kudosas", "Teamwork", "Great release"));

            foreach (var search in new[] { "Petraite", "Kudosas", "Teamwork" })
            {
                Assert.That((await SearchAsync(search)).Kudos, Is.Empty, search);
            }
        }

        [Test]
        public async Task Should_Not_Find_Kudos_Through_A_Hidden_Comment()
        {
            var hidden = Kudos(1, "sender", "Tomas", "Kudosas", "Teamwork", "Secret reason");
            hidden.IsCommentHidden = true;
            GivenKudos(hidden);

            Assert.That((await SearchAsync("Secret")).Kudos, Is.Empty);
        }

        [Test]
        public async Task Should_Only_Find_Kudos_The_Kudos_Page_Lists()
        {
            var pending = Kudos(1, "sender", "Tomas", "Kudosas", "Teamwork", "Thanks one");
            pending.Status = KudosStatus.Pending;
            var minus = Kudos(2, "sender", "Tomas", "Kudosas", "Teamwork", "Thanks two");
            minus.KudosSystemType = KudosTypeEnum.Minus;
            var refund = Kudos(3, "sender", "Tomas", "Kudosas", "Teamwork", "Thanks three");
            refund.KudosSystemType = KudosTypeEnum.Refund;
            var donation = Kudos(4, "sender", "Tomas", "Kudosas", "Teamwork", "Thanks four");
            donation.KudosBasketId = 9;
            GivenKudos(pending, minus, refund, donation, Kudos(5, "sender", "Tomas", "Kudosas", "Teamwork", "Thanks five"));

            var kudos = (await SearchAsync("Thanks")).Kudos;

            Assert.That(kudos.Select(item => item.Id), Is.EqualTo(new[] { "5" }));
        }

        [Test]
        public async Task Should_Find_Projects_And_Approved_Groups_By_Name_Only()
        {
            _projectsDbSet.SetDbSetDataForAsync(new List<Project>
            {
                new Project { Id = 1, Name = "kudos portal", Desc = "For customers", OrganizationId = OrganizationId },
                new Project { Id = 2, Name = "Billing", Desc = "Invoices and kudos", OrganizationId = OrganizationId }
            });
            _groupsDbSet.SetDbSetDataForAsync(new List<GroupEntity>
            {
                new GroupEntity { Id = 1, Name = "kudos committee", Description = "Decides", Status = GroupStatus.Approved, OrganizationId = OrganizationId },
                new GroupEntity { Id = 3, Name = "Board", Description = "Talks about kudos", Status = GroupStatus.Approved, OrganizationId = OrganizationId },
                new GroupEntity { Id = 2, Name = "kudos fans", Description = "Likes", Status = GroupStatus.Pending, OrganizationId = OrganizationId }
            });

            var results = await SearchAsync("kudos");

            Assert.That(results.Projects.Select(item => item.Title), Is.EqualTo(new[] { "kudos portal" }));
            Assert.That(results.Projects.Single().Description, Is.EqualTo("For customers"));
            Assert.That(results.Groups.Select(item => item.Title), Is.EqualTo(new[] { "kudos committee" }));
        }

        [Test]
        public async Task Should_Find_A_Book_Copy_An_Office_Still_Holds()
        {
            var book = new Book { Id = 1, Title = "Clean Code", Author = "Robert Martin", Code = "9780132350884" };
            _bookOfficesDbSet.SetDbSetDataForAsync(new List<BookOffice>
            {
                new BookOffice { Id = 10, Book = book, Office = new Office { Name = "Vilnius" }, Quantity = 2, OrganizationId = OrganizationId },
                new BookOffice { Id = 11, Book = book, Office = new Office { Name = "Kaunas" }, Quantity = 0, OrganizationId = OrganizationId }
            });

            var found = (await SearchAsync("Clean")).Books.Single();

            Assert.That(found.Id, Is.EqualTo("10"));
            Assert.That(found.Title, Is.EqualTo("Clean Code"));
            Assert.That(found.Description, Is.EqualTo("Robert Martin · Vilnius"));
        }

        [Test]
        public async Task Should_Not_Find_A_Book_By_Its_Author_Or_Code()
        {
            var book = new Book { Id = 1, Title = "Clean Code", Author = "Robert Martin", Code = "9780132350884" };
            _bookOfficesDbSet.SetDbSetDataForAsync(new List<BookOffice>
            {
                new BookOffice { Id = 10, Book = book, Office = new Office { Name = "Vilnius" }, Quantity = 2, OrganizationId = OrganizationId }
            });

            Assert.That((await SearchAsync("Martin")).Books, Is.Empty);
            Assert.That((await SearchAsync("9780132350884")).Books, Is.Empty);
        }

        [Test]
        public async Task Should_Find_Published_Polls_And_Videos()
        {
            _pollsDbSet.SetDbSetDataForAsync(new List<Poll>
            {
                new Poll { Id = 1, Title = "Kudos budget", State = PollState.Published, OrganizationId = OrganizationId, Created = Now },
                new Poll { Id = 2, Title = "Kudos draft", State = PollState.Draft, OrganizationId = OrganizationId, Created = Now },
                new Poll { Id = 3, Title = "Kudos idea", State = PollState.Pending, OrganizationId = OrganizationId, Created = Now }
            });
            _videosDbSet.SetDbSetDataForAsync(new List<VideoLibraryItem>
            {
                new VideoLibraryItem { Id = 1, Title = "Kudos explained", RecordedOn = new DateOnly(2026, 5, 4), OrganizationId = OrganizationId }
            });

            var results = await SearchAsync("Kudos");

            Assert.That(results.Polls.Select(item => item.Title), Is.EqualTo(new[] { "Kudos budget" }));
            Assert.That(results.Videos.Single().Date, Is.EqualTo(new DateTime(2026, 5, 4)));
        }

        [Test]
        public async Task Should_Only_Find_Service_Requests_The_User_Raised_Or_Is_Assigned()
        {
            GivenServiceRequests(
                Request(1, "Laptop broken", "Hardware", UserId),
                Request(2, "Laptop slow", "Hardware", "someone-else"),
                Request(3, "Laptop licence", "Software", "someone-else"));
            _serviceRequestCategoriesDbSet.SetDbSetDataForAsync(new List<ServiceRequestCategory>
            {
                new ServiceRequestCategory { Name = "Software", Assignees = new List<ApplicationUser> { new ApplicationUser { Id = UserId } } },
                new ServiceRequestCategory { Name = "Hardware", Assignees = new List<ApplicationUser>() }
            });

            var requests = (await SearchAsync("Laptop")).ServiceRequests;

            Assert.That(requests.Select(item => item.Id), Is.EquivalentTo(new[] { "1", "3" }));
            Assert.That(requests.First(item => item.Id == "1").Description, Is.EqualTo("Hardware · Open"));
        }

        [Test]
        public async Task Should_Let_A_Service_Request_Admin_Find_Every_Request()
        {
            GivenPermissions(BasicPermissions.ServiceRequest, AdministrationPermissions.ServiceRequest);
            GivenServiceRequests(
                Request(1, "Laptop broken", "Hardware", UserId),
                Request(2, "Laptop slow", "Hardware", "someone-else"));

            var requests = (await SearchAsync("Laptop")).ServiceRequests;

            Assert.That(requests.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Leave_The_New_Sections_Empty_Without_Their_Permissions()
        {
            GivenPermissions(BasicPermissions.Event);
            GivenKudos(Kudos(1, "sender", "Tomas", "Kudosas", "Teamwork", "Kudos"));
            GivenServiceRequests(Request(1, "Kudos shop order", "Shop", UserId));
            _projectsDbSet.SetDbSetDataForAsync(new List<Project> { new Project { Id = 1, Name = "Kudos", OrganizationId = OrganizationId } });

            var results = await SearchAsync("Kudos");

            Assert.That(results.Kudos, Is.Empty);
            Assert.That(results.Projects, Is.Empty);
            Assert.That(results.ServiceRequests, Is.Empty);
        }

        [Test]
        public async Task Should_Match_Polls_Videos_And_Service_Requests_On_Their_Title_Only()
        {
            _pollsDbSet.SetDbSetDataForAsync(new List<Poll>
            {
                new Poll { Id = 1, Title = "Budget", Description = "About kudos", State = PollState.Published, OrganizationId = OrganizationId, Created = Now }
            });
            _videosDbSet.SetDbSetDataForAsync(new List<VideoLibraryItem>
            {
                new VideoLibraryItem { Id = 1, Title = "Demo", Description = "About kudos", RecordedOn = new DateOnly(2026, 5, 4), OrganizationId = OrganizationId }
            });
            var request = Request(1, "Laptop broken", "kudos", UserId);
            request.Description = "About kudos";
            GivenServiceRequests(request);

            var results = await SearchAsync("kudos");

            Assert.That(results.Polls, Is.Empty);
            Assert.That(results.Videos, Is.Empty);
            Assert.That(results.ServiceRequests, Is.Empty);
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

        private void GivenPosts(params PostDto[] posts)
        {
            _wallService
                .SearchWallAsync(Arg.Any<string>(), Arg.Any<UserAndOrganizationDto>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(posts);
        }

        private void GivenKudos(params KudosLog[] logs)
        {
            _kudosLogsDbSet.SetDbSetDataForAsync(logs.ToList());
        }

        private void GivenServiceRequests(params ServiceRequest[] requests)
        {
            _serviceRequestsDbSet.SetDbSetDataForAsync(requests.ToList());
        }

        private static KudosLog Kudos(int id, string senderId, string receiverFirstName, string receiverLastName, string type, string comment)
        {
            return new KudosLog
            {
                Id = id,
                OrganizationId = OrganizationId,
                CreatedBy = senderId,
                EmployeeId = $"receiver-{id}",
                Employee = new ApplicationUser { FirstName = receiverFirstName, LastName = receiverLastName },
                KudosTypeName = type,
                KudosSystemType = KudosTypeEnum.Ordinary,
                Status = KudosStatus.Approved,
                Comments = comment,
                Created = Now.AddDays(-id)
            };
        }

        private static ServiceRequest Request(int id, string title, string category, string employeeId)
        {
            return new ServiceRequest
            {
                Id = id,
                Title = title,
                CategoryName = category,
                EmployeeId = employeeId,
                OrganizationId = OrganizationId,
                Status = new ServiceRequestStatus { Title = "Open" },
                Created = Now.AddDays(-id)
            };
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
