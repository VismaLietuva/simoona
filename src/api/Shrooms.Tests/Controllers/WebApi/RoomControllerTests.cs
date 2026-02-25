using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Authentification.Membership;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.ViewModels;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using Shrooms.Tests.Extensions;
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
            _roomController.SetUpControllerForTesting();
        }

        [Test]
        public async Task Room_Get_Should_Return_Correct_View_Model()
        {
            var result = await _roomController.Get(1);
            var room = ((OkObjectResult)result).Value as RoomViewModel;

            Assert.That(room, Is.InstanceOf<RoomViewModel>());
        }

        [Test]
        public async Task Room_Get_Should_Return_Correct_Id()
        {
            var result = await _roomController.Get(1);
            var model = ((OkObjectResult)result).Value as RoomViewModel;

            Assert.That(model.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Room_GetPaged_Should_Return_Correct_Paged_Model()
        {
            var rooms = await _roomController.GetPaged();
            Assert.That(rooms, Is.InstanceOf<PagedViewModel<RoomViewModel>>());
        }

        [Test]
        public async Task Room_GetPaged_Should_Return_Correct_Page_Count()
        {
            var rooms = await _roomController.GetPaged(page: 1, pageSize: 2);
            Assert.That(rooms.PageCount, Is.EqualTo(3));
        }

        [Test]
        public async Task Room_GetPagedByFloor_Should_Return_Correct_Paged_Model()
        {
            var rooms = await _roomController.GetAllRoomsByFloor(floorId: 1);
            Assert.That(rooms, Is.InstanceOf<PagedViewModel<RoomViewModel>>());
        }

        [Test]
        public async Task Room_GetPagedByFloor_Should_Return_Correct_Page_Count()
        {
            var rooms = await _roomController.GetAllRoomsByFloor(floorId: 1, page: 1, pageSize: 2);
            Assert.That(rooms.PageCount, Is.EqualTo(2));
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
                    new()
                    {
                        Id = "1"
                    }
                }
            };

            var userToReturn = _unitOfWork.GetDbContextAs<MockDbContext>().ApplicationUsers.Find(p => p.Id == "1");
            _userManager.FindByIdAsync("1").Returns(Task.FromResult(userToReturn));

            var response = await _roomController.Post(testRoom);

            Assert.That(((StatusCodeResult)response).StatusCode, Is.EqualTo(201));
        }

        [Test]
        public async Task Room_Post_Should_Return_Bad_Request_If_Invalid_Room_Model_Provided()
        {
            var response = await _roomController.Post(null);
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
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
            var response = await _roomController.Put(testRoom);

            Assert.That(((StatusCodeResult)response).StatusCode, Is.EqualTo(201));
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
            var response = await _roomController.Put(testRoom);

            Assert.That(response, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Room_Delete_Should_Return_Not_Found_If_Incorrect_Id_Provided()
        {

            var response = await _roomController.Delete(-1);

            Assert.That(response, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Room_Delete_Should_Return_Ok_If_Room_Deleted_SuccessfullyDeleteReturnOkResponse()
        {

            var response = await _roomController.Delete(1);

            Assert.That(response, Is.InstanceOf<OkResult>());
        }
    }
}
