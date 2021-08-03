using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using AutoMapper;
using Microsoft.AspNet.Identity;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using Shrooms.Tests.Mocks;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class RoomControllerTests
    {
        private IUnitOfWork _unitOfWork;
        private RoomController _roomController;
        private IUserStore<ApplicationUser> _userStore;
        private ShroomsUserManager _userManager;
        private IRepository<ApplicationUser> _applicationUserRepository;
        private IMapper _mapper;

        [SetUp]
        public void TestInitializer()
        {
            _unitOfWork = new MockUnitOfWork();
            _applicationUserRepository = _unitOfWork.GetRepository<ApplicationUser>();
            _userStore = MockIdentity.MockShroomsUserStore(_unitOfWork.DbContext);
            _userManager = MockIdentity.MockUserManager(_userStore, _unitOfWork.DbContext);
            _mapper = ModelMapper.Create();

            _roomController = new RoomController(_mapper, _unitOfWork, _userManager);
            _roomController.ControllerContext = Substitute.For<HttpControllerContext>();
            _roomController.Request = new HttpRequestMessage();
            _roomController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _roomController.Request.SetConfiguration(new HttpConfiguration());
            _roomController.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
        }

        [Test]
        public async Task Room_Get_Should_Return_Correct_View_Model()
        {
            var result = await _roomController.Get(1);
            var room = await result.Content.ReadAsAsync<RoomViewModel>();

            Assert.IsInstanceOf<RoomViewModel>(room);
        }

        [Test]
        public async Task Room_Get_Should_Return_Correct_Id()
        {
            var result = await _roomController.Get(1);
            var model = await result.Content.ReadAsAsync<RoomViewModel>();

            Assert.AreEqual(1, model.Id);
        }

        [Test]
        public async Task Room_GetPaged_Should_Return_Correct_Paged_Model()
        {
            var rooms = await _roomController.GetPaged();
            Assert.IsInstanceOf<PagedViewModel<RoomViewModel>>(rooms);
        }

        [Test]
        public async Task Room_GetPaged_Should_Return_Correct_Page_Count()
        {
            var rooms = await _roomController.GetPaged(page: 1, pageSize: 2);
            Assert.AreEqual(3, rooms.PageCount);
        }

        [Test]
        public async Task Room_GetPagedByFloor_Should_Return_Correct_Paged_Model()
        {
            var rooms = await _roomController.GetAllRoomsByFloor(floorId: 1);
            Assert.IsInstanceOf<PagedViewModel<RoomViewModel>>(rooms);
        }

        [Test]
        public async Task Room_GetPagedByFloor_Should_Return_Correct_Page_Count()
        {
            var rooms = await _roomController.GetAllRoomsByFloor(floorId: 1, page: 1, pageSize: 2);
            Assert.AreEqual(2, rooms.PageCount);
        }

        [Test]
        public async Task Room_Post_Should_Return_Ok_Response_If()
        {
            var testRoom = new RoomPostViewModel
            {
                Id = 7,
                Name = "testName",
                Number = "2",
                Coordinates = "111,222,333",
                FloorId = 1,
                ApplicationUsers = new List<ApplicationUserViewModel>
                {
                    new ApplicationUserViewModel
                    {
                        Id = "1"
                    }
                }
            };

            var userToReturn = _unitOfWork.GetDbContextAs<MockDbContext>().ApplicationUsers.Find(p => p.Id == "1");
            _userManager.FindByIdAsync("1").Returns(Task.FromResult(userToReturn));

            _roomController.Request = new HttpRequestMessage();
            _roomController.Request.SetConfiguration(new HttpConfiguration());
            _roomController.Validate(testRoom);

            var response = await _roomController.Post(testRoom);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [Test]
        public async Task Room_Post_Should_Return_Bad_Request_If_Invalid_Room_Model_Provided()
        {
            var response = await _roomController.Post(null);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Room_Put_Should_Return_Ok_Response_If()
        {
            var applicationUser = await _applicationUserRepository.Get().FirstOrDefaultAsync();
            var applicationUserViewModel = _mapper.Map<ApplicationUser, ApplicationUserViewModel>(applicationUser);

            var testRoom = new RoomPostViewModel
            {
                Id = 1,
                Name = "testName",
                Number = "2",
                Coordinates = "111,222,333",
                FloorId = 1,
                ApplicationUsers = new List<ApplicationUserViewModel>
                {
                    applicationUserViewModel
                }
            };

            // ReSharper disable once PossibleNullReferenceException
            _userManager.FindByIdAsync(applicationUser.Id).Returns(Task.FromResult(applicationUser));

            _roomController.Validate(testRoom);
            var response = await _roomController.Put(testRoom);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [Test]
        public async Task Room_Put_Should_Return_Not_Found_If_Updating_Invalid_Room()
        {
            var testRoom = new RoomPostViewModel
            {
                Id = 100,
                Name = "testName",
                Number = "2",
                Coordinates = "111,222,333",
                FloorId = 1,
                ApplicationUsers = new List<ApplicationUserViewModel>()
            };

            _roomController.Validate(testRoom);
            var response = await _roomController.Put(testRoom);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Room_Delete_Should_Return_Not_Found_If_Incorrect_Id_Provided()
        {
            _roomController.Request = new HttpRequestMessage();
            _roomController.Request.SetConfiguration(new HttpConfiguration());

            var response = await _roomController.Delete(-1);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Room_Delete_Should_Return_Ok_If_Room_Deleted_SuccessfullyDeleteReturnOkResponse()
        {
            _roomController.Request = new HttpRequestMessage();
            _roomController.Request.SetConfiguration(new HttpConfiguration());

            var response = await _roomController.Delete(1);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
