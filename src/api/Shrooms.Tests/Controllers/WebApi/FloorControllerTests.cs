using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
            var floor = ((OkObjectResult)result).Value as FloorViewModel;

            Assert.That(floor, Is.InstanceOf<FloorViewModel>());
        }

        [Test]
        public async Task Floor_Get_Should_Return_Correct_Floor()
        {
            const int id = 1;

            var result = await _floorController.Get(id);
            var floor = ((OkObjectResult)result).Value as FloorViewModel;

            Assert.That(floor.Id, Is.EqualTo(id));
        }

        [Test]
        public async Task Floor_GetByRoom_Should_Return_View_Model()
        {
            const int roomId = 1;
            var floor = await _floorController.GetByRoom(roomId);
            Assert.That(floor, Is.InstanceOf<FloorViewModel>());
        }

        [Test]
        public async Task Floor_GetByRoom_Should_Return_Correct_Floor()
        {
            var floor = await _floorController.GetByRoom(2);
            Assert.That(floor.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Floor_GetByOffice_Should_Return_Floor_View_Model()
        {
            const int roomId = 1;

            var floor = await _floorController.GetByOffice(roomId);
            Assert.That(floor, Is.InstanceOf<IEnumerable<FloorViewModel>>());
        }

        [Test]
        public async Task Floor_GetByOffice_Should_Return_Correct_Floors()
        {
            var floor = await _floorController.GetByOffice(1);
            Assert.That(floor.FirstOrDefault()?.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Floor_GetPaged_Should_Return_Paged_View_Model()
        {
            var pagedFloors = await _floorController.GetPaged(1);
            Assert.That(pagedFloors, Is.InstanceOf<FloorViewPagedModel>());
        }

        [Test]
        public async Task GetManyReturnPageSizedNumberOfFloors()
        {
            const int pageSize = 2;

            var pagedFloors = await _floorController.GetPaged(1, 1, pageSize);
            Assert.That(pagedFloors.PagedList.Count, Is.EqualTo(pageSize));
        }

        [Test]
        public async Task Floor_GetPaged_Should_Return_Correct_Floors_By_Page()
        {
            var pagedFloors = await _floorController.GetPaged(1, 2, 1);
            Assert.That(pagedFloors.PagedList[0].Id, Is.EqualTo(2));
        }

        [Test]
        public async Task Floor_GetPaged_Should_Return_Floors_With_Rooms()
        {
            var pagedFloors = await _floorController.GetPaged(1, 1, 1);
            Assert.That(pagedFloors.PagedList[0].Rooms, Is.Not.Null);
            Assert.That(pagedFloors.PagedList[0].Rooms.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task Floor_GetPaged_Should_Filter_Floors_By_Parameters()
        {
            var pagedFloors = await _floorController.GetPaged(1, 1, 20, "Z-Floor");
            Assert.That(pagedFloors.PagedList.Count, Is.EqualTo(1));
            Assert.That(pagedFloors.PagedList[0].Id, Is.EqualTo(2));
        }

        [Test]
        public async Task Floor_GetPaged_Should_Order_Floors_By_Parameters()
        {
            var pagedFloors = await _floorController.GetPaged(1, 1, 20, string.Empty, "Name descending");
            Assert.That(pagedFloors.PagedList[0].Id, Is.EqualTo(2));
            Assert.That(pagedFloors.PagedList[1].Id, Is.EqualTo(1));
        }

        [Test]
        public async Task Floor_GetPaged_Should_Return_Floors_With_Office()
        {
            var pagedFloors = await _floorController.GetPaged(-1, 1, 1);
            Assert.That(pagedFloors.PagedList[0].Office, Is.Not.Null);
            Assert.That(pagedFloors.PagedList[0].OfficeId, Is.EqualTo(1));
        }

        [Test]
        public async Task Floor_GetPaged_Should_Return_Floors_With_Correct_Users_Count()
        {
            var pagedFloors = await _floorController.GetPaged(-1, 1, 1);
            Assert.That(pagedFloors.PagedList[0].ApplicationUsersCount, Is.EqualTo(3));
        }

        [Test]
        public async Task Floor_Post_Should_Return_Ok_Response()
        {
            var model = new FloorPostViewModel
            {
                Name = "blablabla",
                PictureId = "1"
            };

            var response = await _floorController.Post(model);

            Assert.That(((StatusCodeResult)response).StatusCode, Is.EqualTo(201));
        }

        [Test]
        public async Task Floor_Post_Should_Return_Not_Found_Response()
        {
            var model = new FloorPostViewModel();

            var response = await _floorController.Put(model);

            Assert.That(response, Is.InstanceOf<NotFoundResult>());
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

            var response = await _floorController.Put(model);

            Assert.That(((StatusCodeResult)response).StatusCode, Is.EqualTo(201));
        }

        [Test]
        public async Task Floor_Delete_Should_Return_Not_Found_Response()
        {

            var response = await _floorController.Delete(-1);

            Assert.That(response, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Floor_Delete_Should_Return_Ok_Response()
        {
            var mockRepository = Substitute.For<IRepository<Floor>>();

            var floorToReturn = _unitOfWork.GetDbContextAs<MockDbContext>().Floors.Where(f => f.Id == 1).AsQueryable();
            mockRepository.Get(f => f.Id == 1, includeProperties: "Rooms,Rooms.ApplicationUsers").Returns(floorToReturn);

            var response = await _floorController.Delete(1);

            Assert.That(response, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task Floor_GetAllFloors_Should_Return_Floors()
        {

            var response = await _floorController.GetAllFloors(1);

            var mockOffice = _unitOfWork.GetDbContextAs<MockDbContext>().Offices.Find(o => o.Id == 1);

            Assert.That(mockOffice.Floors.Count, Is.EqualTo(response.PagedList.TotalItemCount));
        }
    }
}
