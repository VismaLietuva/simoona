using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
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
    public class FloorControllerTests
    {
        private IUnitOfWork _unitOfWork;
        private FloorController _floorController;

        [SetUp]
        public void TestInitializer()
        {
            _unitOfWork = new MockUnitOfWork();

            _floorController = new FloorController(ModelMapper.Create(), _unitOfWork);
            _floorController.SetUpControllerForTesting();
        }

        [Test]
        public async Task Floor_Get_Should_Return_View_Model()
        {
            var result = await _floorController.Get(1);
            var floor = await result.Content.ReadAsAsync<FloorViewModel>();

            Assert.IsInstanceOf<FloorViewModel>(floor);
        }

        [Test]
        public async Task Floor_Get_Should_Return_Correct_Floor()
        {
            const int id = 1;

            var result = await _floorController.Get(id);
            var floor = await result.Content.ReadAsAsync<FloorViewModel>();

            Assert.AreEqual(id, floor.Id);
        }

        [Test]
        public async Task Floor_GetByRoom_Should_Return_View_Model()
        {
            const int roomId = 1;
            var floor = await _floorController.GetByRoom(roomId);
            Assert.IsInstanceOf<FloorViewModel>(floor);
        }

        [Test]
        public async Task Floor_GetByRoom_Should_Return_Correct_Floor()
        {
            var floor = await _floorController.GetByRoom(2);
            Assert.AreEqual(1, floor.Id);
        }

        [Test]
        public async Task Floor_GetByOffice_Should_Return_Floor_View_Model()
        {
            const int roomId = 1;

            var floor = await _floorController.GetByOffice(roomId);
            Assert.IsInstanceOf<IEnumerable<FloorViewModel>>(floor);
        }

        [Test]
        public async Task Floor_GetByOffice_Should_Return_Correct_Floors()
        {
            var floor = await _floorController.GetByOffice(1);
            Assert.AreEqual(1, floor.FirstOrDefault()?.Id);
        }

        [Test]
        public async Task Floor_GetPaged_Should_Return_Paged_View_Model()
        {
            var pagedFloors = await _floorController.GetPaged(1);
            Assert.IsInstanceOf<FloorViewPagedModel>(pagedFloors);
        }

        [Test]
        public async Task GetManyReturnPageSizedNumberOfFloors()
        {
            const int pageSize = 2;

            var pagedFloors = await _floorController.GetPaged(1, 1, pageSize);
            Assert.AreEqual(pageSize, pagedFloors.PagedList.Count);
        }

        [Test]
        public async Task Floor_GetPaged_Should_Return_Correct_Floors_By_Page()
        {
            var pagedFloors = await _floorController.GetPaged(1, 2, 1);
            Assert.AreEqual(2, pagedFloors.PagedList[0].Id);
        }

        [Test]
        public async Task Floor_GetPaged_Should_Return_Floors_With_Rooms()
        {
            var pagedFloors = await _floorController.GetPaged(1, 1, 1);
            Assert.IsNotNull(pagedFloors.PagedList[0].Rooms);
            Assert.AreEqual(3, pagedFloors.PagedList[0].Rooms.Count());
        }

        [Test]
        public async Task Floor_GetPaged_Should_Filter_Floors_By_Parameters()
        {
            var pagedFloors = await _floorController.GetPaged(1, 1, 20, "Z-Floor");
            Assert.AreEqual(1, pagedFloors.PagedList.Count);
            Assert.AreEqual(2, pagedFloors.PagedList[0].Id);
        }

        [Test]
        public async Task Floor_GetPaged_Should_Order_Floors_By_Parameters()
        {
            var pagedFloors = await _floorController.GetPaged(1, 1, 20, string.Empty, "Name descending");
            Assert.AreEqual(2, pagedFloors.PagedList[0].Id);
            Assert.AreEqual(1, pagedFloors.PagedList[1].Id);
        }

        [Test]
        public async Task Floor_GetPaged_Should_Return_Floors_With_Office()
        {
            var pagedFloors = await _floorController.GetPaged(-1, 1, 1);
            Assert.IsNotNull(pagedFloors.PagedList[0].Office);
            Assert.AreEqual(1, pagedFloors.PagedList[0].OfficeId);
        }

        [Test]
        public async Task Floor_GetPaged_Should_Return_Floors_With_Correct_Users_Count()
        {
            var pagedFloors = await _floorController.GetPaged(-1, 1, 1);
            Assert.AreEqual(3, pagedFloors.PagedList[0].ApplicationUsersCount);
        }

        [Test]
        public async Task Floor_Post_Should_Return_Ok_Response()
        {
            var model = new FloorPostViewModel
            {
                Name = "blablabla",
                PictureId = "1"
            };

            _floorController.Request = new HttpRequestMessage();
            _floorController.Request.SetConfiguration(new HttpConfiguration());
            _floorController.Validate(model);

            var response = await _floorController.Post(model);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [Test]
        public async Task Floor_Post_Should_Return_Not_Found_Response()
        {
            var model = new FloorPostViewModel();
            _floorController.Request = new HttpRequestMessage();
            _floorController.Request.SetConfiguration(new HttpConfiguration());
            _floorController.Validate(model);

            var response = await _floorController.Put(model);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Floor_Put_Should_Return_Ok_Response()
        {
            var model = new FloorPostViewModel
            {
                Id = 1,
                Name = "TestFloor",
                PictureId = "1"
            };

            _floorController.Request = new HttpRequestMessage();
            _floorController.Request.SetConfiguration(new HttpConfiguration());
            _floorController.Validate(model);

            var response = await _floorController.Put(model);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [Test]
        public async Task Floor_Delete_Should_Return_Not_Found_Response()
        {
            _floorController.Request = new HttpRequestMessage();
            _floorController.Request.SetConfiguration(new HttpConfiguration());

            var response = await _floorController.Delete(-1);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Floor_Delete_Should_Return_Ok_Response()
        {
            var mockRepository = Substitute.For<IRepository<Floor>>();

            var floorToReturn = _unitOfWork.GetDbContextAs<MockDbContext>().Floors.Where(f => f.Id == 1).AsQueryable();
            mockRepository.Get(f => f.Id == 1, includeProperties: "Rooms,Rooms.ApplicationUsers").Returns(floorToReturn);

            _floorController.Request = new HttpRequestMessage();
            _floorController.Request.SetConfiguration(new HttpConfiguration());

            var response = await _floorController.Delete(1);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Floor_GetAllFloors_Should_Return_Floors()
        {
            _floorController.Request = new HttpRequestMessage();
            _floorController.Request.SetConfiguration(new HttpConfiguration());

            var response = await _floorController.GetAllFloors(1);

            var mockOffice = _unitOfWork.GetDbContextAs<MockDbContext>().Offices.Find(o => o.Id == 1);

            Assert.AreEqual(response.PagedList.TotalItemCount, mockOffice.Floors.Count);
        }
    }
}
