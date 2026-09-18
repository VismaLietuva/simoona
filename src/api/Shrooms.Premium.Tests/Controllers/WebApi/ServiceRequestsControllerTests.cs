using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;
using Shrooms.Premium.Domain.Services.KudosShop;
using Shrooms.Premium.Domain.Services.ServiceRequests;
using Shrooms.Premium.Presentation.Api.Controllers;
using Shrooms.Premium.Tests.Mocks;
using Shrooms.Premium.Tests.ModelMappings;
using Shrooms.Tests.Extensions;

namespace Shrooms.Premium.Tests.Controllers.WebApi
{
    [TestFixture]
    public class ServiceRequestsControllerTests
    {
        private ServiceRequestsController _sut;

        private const string CurrentUserId = "1";

        private IPermissionService _permissionService;
        private IServiceRequestService _serviceRequestService;
        private IRepository<ServiceRequest> _serviceRequestRepository;
        private List<ServiceRequest> _requests;

        [SetUp]
        public void TestInitializers()
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            _serviceRequestRepository = Substitute.For<IRepository<ServiceRequest>>();
            unitOfWork.GetRepository<ServiceRequest>().Returns(_serviceRequestRepository);

            _permissionService = Substitute.For<IPermissionService>();
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .Returns(true);

            _serviceRequestService = Substitute.For<IServiceRequestService>();
            _serviceRequestService.GetCategoriesAsync().Returns(new List<ServiceRequestCategoryDto>());

            _requests = new List<ServiceRequest>
            {
                Request(1, "Broken laptop screen", "Ann", "Smith"),
                Request(2, "New keyboard", "Bob", "Jones"),
            };

            // The repository normally hands the filter to the database; here it
            // is applied in memory so the expression itself is what's tested.
            // Note: in memory string.Contains is ordinal, so matching is
            // case-sensitive here while production runs a case-insensitive
            // collation. Expectations below use the seeded casing on purpose.
            _serviceRequestRepository
                .Get(
                    Arg.Any<Expression<Func<ServiceRequest, bool>>>(),
                    Arg.Any<int>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<int?>())
                .Returns(call =>
                {
                    var filter = call.Arg<Expression<Func<ServiceRequest, bool>>>();
                    return new TestDbAsyncEnumerable<ServiceRequest>(_requests.Where(filter.Compile()));
                });

            _sut = new ServiceRequestsController(
                ModelMapper.Create(),
                unitOfWork,
                Substitute.For<IKudosShopService>(),
                _permissionService,
                _serviceRequestService,
                Substitute.For<IServiceRequestExportService>());
            _sut.SetUpControllerForTesting();
        }

        // The reported bug: first and last name live in separate columns, so
        // looking for the whole phrase in either one matched nothing.
        [Test]
        public async Task GetPagedFiltered_SearchSpansFirstAndLastName_ReturnsRequest()
        {
            var titles = await SearchTitlesAsync("Ann Smith");

            ClassicAssert.AreEqual(new[] { "Broken laptop screen" }, titles);
        }

        [Test]
        public async Task GetPagedFiltered_SearchIsOneName_ReturnsRequest()
        {
            var titles = await SearchTitlesAsync("Smith");

            ClassicAssert.AreEqual(new[] { "Broken laptop screen" }, titles);
        }

        // Words are matched one by one, so they need not sit next to each other
        // in the title the way a single substring match demanded.
        [Test]
        public async Task GetPagedFiltered_SearchWordsAreNotAdjacentInTitle_ReturnsRequest()
        {
            var titles = await SearchTitlesAsync("screen Broken");

            ClassicAssert.AreEqual(new[] { "Broken laptop screen" }, titles);
        }

        [Test]
        public async Task GetPagedFiltered_SearchMixesTitleAndName_ReturnsRequest()
        {
            var titles = await SearchTitlesAsync("keyboard Jones");

            ClassicAssert.AreEqual(new[] { "New keyboard" }, titles);
        }

        // Every word has to match something, otherwise a surname belonging to
        // someone else would widen the result set instead of narrowing it.
        [Test]
        public async Task GetPagedFiltered_OneWordMatchesNothing_ReturnsNothing()
        {
            var titles = await SearchTitlesAsync("Ann Jones");

            ClassicAssert.IsEmpty(titles);
        }

        [Test]
        public async Task GetPagedFiltered_SearchIsEmpty_ReturnsEverything()
        {
            var titles = await SearchTitlesAsync(string.Empty);

            ClassicAssert.AreEqual(2, titles.Count);
        }

        // The input arrives straight from the search box, so a stray space left
        // behind while typing must not be treated as a word.
        [Test]
        public async Task GetPagedFiltered_SearchHasTrailingSpace_ReturnsRequest()
        {
            var titles = await SearchTitlesAsync("Ann ");

            ClassicAssert.AreEqual(new[] { "Broken laptop screen" }, titles);
        }
        // Each word costs a SQL parameter and a CHARINDEX call per column, so
        // anything past the cap is dropped rather than queried.
        [Test]
        public async Task GetPagedFiltered_SearchExceedsWordCap_IgnoresTheExtraWords()
        {
            var search = string.Join(" ", Enumerable.Repeat("Ann", WebApiConstants.MaxSearchWords)) + " nonsense";

            var titles = await SearchTitlesAsync(search);

            ClassicAssert.AreEqual(new[] { "Broken laptop screen" }, titles);
        }

        // Everything above runs as an administrator. The rest cover
        // filterForCurrentUser, the branch an ordinary employee hits.

        [Test]
        public async Task GetPagedFiltered_RegularUserSearchesOwnRequestByTitle_ReturnsRequest()
        {
            SeedRequestsOwnedByCurrentUser();
            SignInAsRegularUser();

            var titles = await SearchTitlesAsync("keyboard");

            ClassicAssert.AreEqual(new[] { "New keyboard" }, titles);
        }

        // That clause is already scoped to the viewer's own requests, so matching
        // their own name there would return everything they ever filed.
        [Test]
        public async Task GetPagedFiltered_RegularUserSearchesOwnName_ReturnsNothing()
        {
            SeedRequestsOwnedByCurrentUser();
            SignInAsRegularUser();

            var titles = await SearchTitlesAsync("Smith");

            ClassicAssert.IsEmpty(titles);
        }

        [Test]
        public async Task GetPagedFiltered_RegularUserSearchesSomeoneElsesRequest_ReturnsNothing()
        {
            SignInAsRegularUser();

            var titles = await SearchTitlesAsync("keyboard");

            ClassicAssert.IsEmpty(titles);
        }

        // An assignee does see other people's requests, and there the name
        // columns are what makes the requester searchable.
        [Test]
        public async Task GetPagedFiltered_AssigneeSearchesRequesterName_ReturnsRequest()
        {
            SignInAsRegularUser("Other");

            var titles = await SearchTitlesAsync("Bob Jones");

            ClassicAssert.AreEqual(new[] { "New keyboard" }, titles);
        }

        [Test]
        public async Task GetPagedFiltered_AssigneeSearchesOutsideTheirCategories_ReturnsNothing()
        {
            SignInAsRegularUser("Hardware");

            var titles = await SearchTitlesAsync("Bob Jones");

            ClassicAssert.IsEmpty(titles);
        }

        private void SignInAsRegularUser(params string[] assigneeCategories)
        {
            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Any<string>())
                .Returns(false);

            _serviceRequestService.GetCategoriesAsync().Returns(assigneeCategories
                .Select(name => new ServiceRequestCategoryDto
                {
                    Name = name,
                    Assignees = new[] { new ApplicationUserMinimalDto { Id = CurrentUserId } }
                })
                .ToList());
        }

        private void SeedRequestsOwnedByCurrentUser()
        {
            _requests = new List<ServiceRequest>
            {
                Request(1, "Broken laptop screen", "Ann", "Smith", CurrentUserId),
                Request(2, "New keyboard", "Ann", "Smith", CurrentUserId)
            };
        }

        private static ServiceRequest Request(int id, string title, string firstName, string lastName, string employeeId = null)
        {
            return new ServiceRequest
            {
                Id = id,
                Title = title,
                EmployeeId = employeeId ?? firstName,
                Employee = new ApplicationUser { Id = firstName, FirstName = firstName, LastName = lastName },
                Priority = new ServiceRequestPriority { Title = "Low" },
                Status = new ServiceRequestStatus { Title = "Open" },
                CategoryName = "Other",
            };
        }

        private async Task<List<string>> SearchTitlesAsync(string search)
        {
            var page = await _sut.GetPagedFiltered(search: search);
            return page.PagedList.Select(request => request.Title).ToList();
        }
    }
}
