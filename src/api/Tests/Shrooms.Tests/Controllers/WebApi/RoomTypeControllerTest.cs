using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using NSubstitute;
using NUnit.Framework;
using Shrooms.API.Controllers;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.UnitTests.Mocks;
using Shrooms.UnitTests.ModelMappings;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.PostModels;

namespace Shrooms.UnitTests.Controllers.WebApi
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
            _roomController.ControllerContext = Substitute.For<HttpControllerContext>();
            _roomController.Request = new HttpRequestMessage();
            _roomController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _roomController.Request.SetConfiguration(new HttpConfiguration());
            _roomController.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
        }

        [Test]
        public async Task RoomType_Get_Should_Return_Correct_Id()
        {
            var result = _roomController.Get(1);
            var model = await result.Content.ReadAsAsync<RoomTypeViewModel>();

            Assert.AreEqual(1, model.Id);
        }

        [Test]
        public async Task RoomType_Get_Should_Return_MeetingRoomTypeViewModel()
        {
            var result = _roomController.Get(2);
            var model = await result.Content.ReadAsAsync<RoomTypeViewModel>();

            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Meeting Room", model.Name);
            Assert.AreEqual("#FF0000", model.Color);
        }

        [Test]
        public void RoomType_Get_Should_Return_Bad_Request_If_Incorrect_Id_Is_Provided()
        {
            var model = _roomController.Get(-1);

            Assert.AreEqual(HttpStatusCode.BadRequest, model.StatusCode);
        }

        [Test]
        public void RoomType_GetByFloor_Should_Return_List_Of_RoomTypeViewModel()
        {
            var model = _roomController.GetByFloor(1);

            Assert.IsInstanceOf<IEnumerable<RoomTypeViewModel>>(model);
        }

        [Test]
        public void RoomType_GetAll_Should_Return_Room_Types_Ordered_By_Name()
        {
            var models = _roomController.GetAll(orderBy: "Name") as List<RoomTypeViewModel>;

            Assert.AreEqual(5, models.Count);
            Assert.AreEqual("Kitchen", models[0].Name);
            Assert.AreEqual("Meeting Room", models[1].Name);
            Assert.AreEqual("Room", models[2].Name);
            Assert.AreEqual("Unknown", models[3].Name);
            Assert.AreEqual("WC", models[4].Name);
        }

        [Test]
        public void RoomType_Validate_Should_Return_False_If_Invalid_Model_Data()
        {
            var model = new RoomTypePostViewModel();

            _roomController.Validate(model);

            Assert.AreEqual(false, _roomController.ModelState.IsValid);
        }

        [Test]
        public void RoomType_Create_Should_Return_Conflict_If_Room_Already_Exists()
        {
            var model = new RoomTypePostViewModel();

            model.Id = 1;
            model.Color = "#FFFFFF";
            model.Name = "-";

            _roomController.Validate(model);
            var response = _roomController.Post(model);

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Test]
        public void RoomType_Create_Should_Return_Conflict_If_Invalid_Data_Provided()
        {
            var response = _roomController.Post(null);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public void RoomType_Create_Should_Return_Created_Room_If_Successfully_Created()
        {
            var model = new RoomTypePostViewModel();

            model.Color = "#ABCDEF";
            model.Name = "TEST create";

            _roomController.Validate(model);
            var response = _roomController.Post(model);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [Test]
        public async Task RoomType_Update_Should_Return_Updated_Room_If_Successfully_Updated()
        {
            var result = _roomController.Get(2);
            var model = await result.Content.ReadAsAsync<RoomTypeViewModel>();

            model.Name = "Changed";
            model.Color = "#FEDCBA";

            var result2 = _roomController.Get(2);
            var model2 = await result2.Content.ReadAsAsync<RoomTypeViewModel>();

            Assert.AreNotEqual(model.Name, model2.Name);
            Assert.AreNotEqual(model.Color, model2.Color);
        }

        [Test]
        public void RoomType_Put_Should_Return_Not_Found_If_Room_Not_Exists()
        {
            var model = new RoomTypePostViewModel();

            _roomController.Validate(model);
            var response = _roomController.Put(model);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public void RoomType_Put_Should_Return_Ok_If_Updated_Successfully()
        {
            var model = new RoomTypePostViewModel
            {
                Id = 1,
                Name = "TestType",
                IconId = "1"
            };

            _roomController.Validate(model);
            var response = _roomController.Put(model);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [Test]
        public void RoomType_Delete_Should_Return_Not_Found_If_Incorrect_Id_Provided()
        {
            var message = _roomController.Delete(0);

            Assert.AreEqual(HttpStatusCode.NotFound, message.StatusCode);
        }

        [Test]
        public void RoomType_Delete_Should_Return_Ok_And_Room_Type_If_Successfully_Deleted()
        {
            var message = _roomController.Delete(1);

            Assert.AreEqual(HttpStatusCode.OK, message.StatusCode);

            var model = _roomController.Get(1);

            Assert.AreEqual(HttpStatusCode.BadRequest, model.StatusCode);
        }

        [Test]
        public void RoomType_GetPaged_Should_Return_Paged_List()
        {
            int pageIndex = 2, pageSize = 2;
            var model = _roomController.GetPaged(page: pageIndex, pageSize: pageSize);

            Assert.AreEqual(3, model.PageCount);
            Assert.AreEqual(pageSize, model.PageSize);
            Assert.AreEqual(pageSize, model.PagedList.Count);
        }

        [Test]
        [TestCase("name", WebApiConstants.DefaultPageSize, "Kitchen", "#00FF00")]
        [TestCase("color", WebApiConstants.DefaultPageSize, "Unknown", "#000000")]
        public void RoomType_GetPaged_Should_Return_Sorted_And_Paged_Room_Types(string sort, int pageSize, string firstName, string firstColor)
        {
            var model = _roomController.GetPaged(sort: sort, pageSize: pageSize);

            Assert.AreEqual(firstName, model.PagedList[0].Name);
            Assert.AreEqual(firstColor, model.PagedList[0].Color);
        }
    }
}