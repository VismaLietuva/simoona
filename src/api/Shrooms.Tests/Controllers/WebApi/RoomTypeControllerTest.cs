using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.Mocks;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class RoomTypeControllerTest
    {
        private IUnitOfWork _unitOfWork;
        private RoomTypeController _roomController;

        [SetUp]
        public void TestInitializer()
        {
            _unitOfWork = new MockUnitOfWork();

            _roomController = new RoomTypeController(ModelMapper.Create(), _unitOfWork);
            _roomController.SetUpControllerForTesting();
        }

        [Test]
        public async Task RoomType_Get_Should_Return_Correct_Id()
        {
            var result = await _roomController.Get(1);
            var model = result.GetContent<RoomTypeViewModel>();

            Assert.That(model.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task RoomType_Get_Should_Return_MeetingRoomTypeViewModel()
        {
            var result = await _roomController.Get(2);
            var model = result.GetContent<RoomTypeViewModel>();

            Assert.That(model.Id, Is.EqualTo(2));
            Assert.That(model.Name, Is.EqualTo("Meeting Room"));
            Assert.That(model.Color, Is.EqualTo("#FF0000"));
        }

        [Test]
        public async Task RoomType_Get_Should_Return_Bad_Request_If_Incorrect_Id_Is_Provided()
        {
            var model = await _roomController.Get(-1);

            Assert.That(model.GetStatusCode(), Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task RoomType_GetByFloor_Should_Return_List_Of_RoomTypeViewModel()
        {
            var model = await _roomController.GetByFloor(1);

            Assert.That(model, Is.InstanceOf<IEnumerable<RoomTypeViewModel>>());
        }

        [Test]
        public async Task RoomType_GetAll_Should_Return_Room_Types_Ordered_By_Name()
        {
            var models = await _roomController.GetAll(orderBy: "Name") as List<RoomTypeViewModel>;

            Assert.That(models, Is.Not.Null);
            Assert.That(models.Count, Is.EqualTo(5));
            Assert.That(models[0].Name, Is.EqualTo("Kitchen"));
            Assert.That(models[1].Name, Is.EqualTo("Meeting Room"));
            Assert.That(models[2].Name, Is.EqualTo("Room"));
            Assert.That(models[3].Name, Is.EqualTo("Unknown"));
            Assert.That(models[4].Name, Is.EqualTo("WC"));
        }

        [Test]
        public void RoomType_Validate_Should_Return_False_If_Invalid_Model_Data()
        {
            var model = new RoomTypePostViewModel();

            _roomController.Validate(model);

            Assert.That(_roomController.ModelState.IsValid, Is.EqualTo(false));
        }

        [Test]
        public async Task RoomType_Create_Should_Return_Conflict_If_Room_Already_Exists()
        {
            var model = new RoomTypePostViewModel();

            model.Id = 1;
            model.Color = "#FFFFFF";
            model.Name = "-";

            _roomController.Validate(model);
            var response = await _roomController.Post(model);

            Assert.That(response.GetStatusCode(), Is.EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public async Task RoomType_Create_Should_Return_Conflict_If_Invalid_Data_Provided()
        {
            var response = await _roomController.Post(null);

            Assert.That(response.GetStatusCode(), Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task RoomType_Create_Should_Return_Created_Room_If_Successfully_Created()
        {
            var model = new RoomTypePostViewModel();

            model.Color = "#ABCDEF";
            model.Name = "TEST create";

            _roomController.Validate(model);
            var response = await _roomController.Post(model);

            Assert.That(response.GetStatusCode(), Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task RoomType_Update_Should_Return_Updated_Room_If_Successfully_Updated()
        {
            var result = await _roomController.Get(2);
            var model = result.GetContent<RoomTypeViewModel>();

            model.Name = "Changed";
            model.Color = "#FEDCBA";

            var result2 = await _roomController.Get(2);
            var model2 = result2.GetContent<RoomTypeViewModel>();

            Assert.That(model2.Name, Is.Not.EqualTo(model.Name));
            Assert.That(model2.Color, Is.Not.EqualTo(model.Color));
        }

        [Test]
        public async Task RoomType_Put_Should_Return_Not_Found_If_Room_Not_Exists()
        {
            var model = new RoomTypePostViewModel();

            _roomController.Validate(model);
            var response = await _roomController.Put(model);

            Assert.That(response.GetStatusCode(), Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task RoomType_Put_Should_Return_Ok_If_Updated_Successfully()
        {
            var model = new RoomTypePostViewModel
            {
                Id = 1,
                Name = "TestType",
                IconId = "1"
            };

            _roomController.Validate(model);
            var response = await _roomController.Put(model);

            Assert.That(response.GetStatusCode(), Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task RoomType_Delete_Should_Return_Not_Found_If_Incorrect_Id_Provided()
        {
            var message = await _roomController.Delete(0);

            Assert.That(message.GetStatusCode(), Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task RoomType_Delete_Should_Return_Ok_And_Room_Type_If_Successfully_Deleted()
        {
            var message = await _roomController.Delete(1);

            Assert.That(message.GetStatusCode(), Is.EqualTo(HttpStatusCode.OK));

            var model = await _roomController.Get(1);

            Assert.That(model.GetStatusCode(), Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task RoomType_GetPaged_Should_Return_Paged_List()
        {
            const int pageIndex = 2;
            const int pageSize = 2;
            var model = await _roomController.GetPaged(page: pageIndex, pageSize: pageSize);

            Assert.That(model.PageCount, Is.EqualTo(3));
            Assert.That(model.PageSize, Is.EqualTo(pageSize));
            Assert.That(model.PagedList.Count, Is.EqualTo(pageSize));
        }

        [Test]
        [TestCase("name", WebApiConstants.DefaultPageSize, "Kitchen", "#00FF00")]
        [TestCase("color", WebApiConstants.DefaultPageSize, "Unknown", "#000000")]
        public async Task RoomType_GetPaged_Should_Return_Sorted_And_Paged_Room_Types(string sort, int pageSize, string firstName, string firstColor)
        {
            var model = await _roomController.GetPaged(pageSize: pageSize, sort: sort);

            Assert.That(model.PagedList[0].Name, Is.EqualTo(firstName));
            Assert.That(model.PagedList[0].Color, Is.EqualTo(firstColor));
        }
    }
}
