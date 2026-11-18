using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;
using Shrooms.Premium.Domain.Services.KudosShop;
using Shrooms.Premium.Domain.Services.ServiceRequests;
using Shrooms.Premium.Presentation.Api.Controllers;
using Shrooms.Premium.Presentation.WebViewModels.ServiceRequests;
using Shrooms.Premium.Tests.Mocks;
using Shrooms.Premium.Tests.ModelMappings;
using Shrooms.Presentation.Api.Helpers;
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

        // The responses below are serialized with the API's own settings, so they
        // hold what a client gets. The requester used to be the ApplicationUser
        // entity itself, which put every requester's password hash and security
        // stamp in the list.

        [Test]
        public void Get_RequesterHasIdentityData_ResponseCarriesOnlyDisplayFields()
        {
            _requests = new List<ServiceRequest> { RequestWithFullyLoadedRelations() };

            var result = (OkObjectResult)_sut.Get(1, "Employee, KudosShopItem");
            var json = ToApiJson(result.Value);

            AssertNoIdentityFields(json);
            AssertDisplayOnlyRequester(json["employee"]);
            AssertShopItemWithoutOrganization(json["kudosShopItem"]);
        }

        [Test]
        public async Task GetPagedFiltered_RequesterHasIdentityData_ResponseCarriesOnlyDisplayFields()
        {
            _requests = new List<ServiceRequest> { RequestWithFullyLoadedRelations() };

            var page = await _sut.GetPagedFiltered("Employee, KudosShopItem");
            var json = ToApiJson(page);

            AssertNoIdentityFields(json);
            var request = json["pagedList"].Single();
            AssertDisplayOnlyRequester(request["employee"]);
            AssertShopItemWithoutOrganization(request["kudosShopItem"]);
        }

        // The list shows "Deleted employee" when the requester is gone.
        [Test]
        public async Task GetPagedFiltered_RequesterIsMissing_EmployeeIsNull()
        {
            var request = RequestWithFullyLoadedRelations();
            request.Employee = null;
            _requests = new List<ServiceRequest> { request };

            var json = ToApiJson(await _sut.GetPagedFiltered("Employee"));

            ClassicAssert.AreEqual(JTokenType.Null, json["pagedList"].Single()["employee"].Type);
        }

        // The client chooses which navigations are loaded (includeProperties), so
        // any Identity type reachable from the response type can end up in the
        // JSON, whether or not a test happens to fill it.
        [Test]
        public void ServiceRequestViewModel_ReachesNoIdentityType()
        {
            var identityTypes = ReachableTypes(typeof(ServiceRequestViewModel)).Where(IsIdentityType).ToList();

            ClassicAssert.IsEmpty(identityTypes, string.Join(", ", identityTypes.Select(t => t.Name)));
        }

        private static readonly string[] IdentityFieldNames =
        {
            "passwordHash", "securityStamp", "concurrencyStamp", "userName", "normalizedUserName", "email",
            "normalizedEmail", "emailConfirmed", "phoneNumber", "phoneNumberConfirmed", "twoFactorEnabled",
            "lockoutEnd", "lockoutEnabled", "accessFailedCount"
        };

        private static ServiceRequest RequestWithFullyLoadedRelations()
        {
            var organization = new Organization { Id = 2, Name = "Visma", BookAppAuthorizationGuid = "book-app-secret" };

            return new ServiceRequest
            {
                Id = 1,
                Title = "Broken laptop screen",
                EmployeeId = "requester",
                Employee = new ApplicationUser
                {
                    Id = "requester",
                    FirstName = "Ann",
                    LastName = "Smith",
                    PictureId = "ann.jpg",
                    UserName = "ann@example.com",
                    NormalizedUserName = "ANN@EXAMPLE.COM",
                    Email = "ann@example.com",
                    NormalizedEmail = "ANN@EXAMPLE.COM",
                    PhoneNumber = "+37060000000",
                    PasswordHash = "AQAAAAEAACcQAAAAEPasswordHash",
                    SecurityStamp = "SECURITYSTAMP",
                    ConcurrencyStamp = "CONCURRENCYSTAMP",
                    Organization = organization
                },
                Priority = new ServiceRequestPriority { Id = 1, Title = "Low" },
                Status = new ServiceRequestStatus { Id = 1, Title = "Open" },
                CategoryName = "Kudos",
                KudosShopItemId = 7,
                KudosShopItem = new KudosShopItem
                {
                    Id = 7,
                    Name = "Mug",
                    Price = 50,
                    Description = "Ceramic",
                    PictureId = "mug.jpg",
                    OrganizationId = 2,
                    Organization = organization
                }
            };
        }

        private static JObject ToApiJson(object value)
        {
            return JObject.FromObject(value, JsonSerializer.Create(ApiJsonSettings.Create()));
        }

        private static void AssertNoIdentityFields(JObject json)
        {
            var leaked = json.Descendants()
                .OfType<JProperty>()
                .Where(property => IdentityFieldNames.Contains(property.Name))
                .Select(property => property.Path)
                .ToList();

            ClassicAssert.IsEmpty(leaked, string.Join(", ", leaked));
            StringAssert.DoesNotContain("AQAAAAEAACcQAAAAEPasswordHash", json.ToString());
            StringAssert.DoesNotContain("SECURITYSTAMP", json.ToString());
            StringAssert.DoesNotContain("book-app-secret", json.ToString());
        }

        // The fields the requester had before, under the same names and values.
        private static void AssertDisplayOnlyRequester(JToken employee)
        {
            ClassicAssert.AreEqual(
                new[] { "firstName", "fullName", "id", "lastName", "pictureId" },
                ((JObject)employee).Properties().Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
            ClassicAssert.AreEqual("requester", (string)employee["id"]);
            ClassicAssert.AreEqual("Ann", (string)employee["firstName"]);
            ClassicAssert.AreEqual("Smith", (string)employee["lastName"]);
            ClassicAssert.AreEqual("Ann Smith", (string)employee["fullName"]);
            ClassicAssert.AreEqual("ann.jpg", (string)employee["pictureId"]);
        }

        private static void AssertShopItemWithoutOrganization(JToken shopItem)
        {
            ClassicAssert.AreEqual(
                new[] { "description", "id", "name", "pictureId", "price" },
                ((JObject)shopItem).Properties().Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
            ClassicAssert.AreEqual(7, (int)shopItem["id"]);
            ClassicAssert.AreEqual("Mug", (string)shopItem["name"]);
            ClassicAssert.AreEqual(50, (int)shopItem["price"]);
        }

        private static bool IsIdentityType(Type type)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                if (current.Namespace == typeof(IdentityUser).Namespace)
                {
                    return true;
                }
            }

            return false;
        }

        // Types a serializer can reach from the root through public properties,
        // looking through collections and nullables.
        private static HashSet<Type> ReachableTypes(Type root)
        {
            var seen = new HashSet<Type>();
            var pending = new Stack<Type>(new[] { root });

            while (pending.Count > 0)
            {
                var type = pending.Pop();
                if (type.IsGenericType)
                {
                    foreach (var argument in type.GetGenericArguments())
                    {
                        pending.Push(argument);
                    }
                }

                if (type.IsPrimitive || type == typeof(string) || type.Namespace == "System" || !seen.Add(type))
                {
                    continue;
                }

                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    pending.Push(property.PropertyType);
                }
            }

            return seen;
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
