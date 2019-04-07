using System.Collections.Generic;
using System.Linq;
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
using Shrooms.API.Controllers;
using Shrooms.API.Controllers.WebApi;
using Shrooms.Authentification;
using Shrooms.Constants.WebApi;
using Shrooms.DataLayer;
using Shrooms.EntityModels.Models;
using Shrooms.ModelMappings;
using Shrooms.UnitTests.ModelMappings;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.PostModels;

namespace Shrooms.UnitTests.Controllers.WebApi
{
    [TestFixture]
    internal class OfficeControllerTests
    {
        private IUnitOfWork _unitOfWork;
        private OfficeController _officeController;
        private IUserStore<ApplicationUser> _userStore;
        private IRoleStore<ApplicationRole, string> _roleStore;
        private ShroomsUserManager _userManager;
        private ShroomsRoleManager _roleManager;
        private IMapper _mapper;

        [SetUp]
        public void TestInitializer()
        {
            _unitOfWork = new MockUnitOfWork();
            _userStore = MockIdentity.MockShroomsUserStore(_unitOfWork.DbContext);
            _roleStore = MockIdentity.MockRoleStore();
            _userManager = MockIdentity.MockUserManager(_userStore, _unitOfWork.DbContext);
            _roleManager = MockIdentity.MockRoleManager(_roleStore);
            _mapper = ModelMapper.Create();

            _officeController = new OfficeController(_mapper, _unitOfWork);
            _officeController.ControllerContext = Substitute.For<HttpControllerContext>();
            _officeController.Request = new HttpRequestMessage();
            _officeController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _officeController.Request.SetConfiguration(new HttpConfiguration());
            _officeController.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
        }

        [Test]
        public void Office_GetAll_Should_Return_All_Offices()
        {
            var result = _officeController.GetAll() as List<OfficeViewModel>;
            Assert.AreEqual(result.Count, 4);
        }

        [Test]
        public async Task Office_Get_Returns_Correct_Office()
        {
            var result = _officeController.Get(1);
            var model = await result.Content.ReadAsAsync<OfficeViewModel>();
            Assert.AreEqual(model.Name, "B-Office");
        }

        [Test]
        public void Office_Get_Should_Return_Bad_Request_If_Giving_Incorrect_Id()
        {
            var result = _officeController.Get(0);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public void Office_GetDefault_Should_Return_View_Model()
        {
            var model = _officeController.GetDefault();
            Assert.IsInstanceOf<OfficeViewModel>(model);
        }

        [Test]
        public void Office_GetPaged_Should_Return_List_Of_All_Offices()
        {
            var result = _officeController.GetPaged();
            Assert.AreEqual(4, result.PagedList.Count);
        }

        [Test]
        [TestCase("City", ConstWebApi.DefaultPageSize, "A-Office")]
        [TestCase("StreetBuilding", ConstWebApi.DefaultPageSize, "B-Office")]
        public void Office_GetPaged_Should_Return_Sorted_List(string sort, int amountResult, string officeNameResult)
        {
            var result = _officeController.GetPaged(sort: sort);
            Assert.AreEqual(result.PagedList.FirstOrDefault().Name, officeNameResult);
        }

        [Test]
        public void Office_GetPaged_Should_Return_Searched_Offices()
        {
            var result = _officeController.GetPaged(s: "B-Office");
            Assert.AreEqual(result.PagedList.Count, 1);
            Assert.AreEqual(result.PagedList.FirstOrDefault().Name, "B-Office");
        }

        [Test]
        public void Office_Put_Should_Return_Bad_Request_If_Model_State_Is_Not_Valid()
        {
            _officeController.ModelState.AddModelError("key", "error message");
            var result = _officeController.Put(null);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
        }

        [Test]
        public void Office_Put_Should_Return_Bad_Request_If()
        {
            var result = _officeController.Put(null);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public void Office_Delete_Should_Return_Not_Found_If_Office_Was_Deleted()
        {
            var result = _officeController.Delete(new int());
            Assert.AreEqual(result.StatusCode, HttpStatusCode.NotFound);
        }

        [Test]
        public void Office_Post_Should_Return_New_Office_And_Ok_Response_If_Added_Successfully()
        {
            var testOffice = new OfficePostViewModel
            {
                Name = "NewOffice",
            };

            var result = _officeController.Post(testOffice);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.Created);
        }

        [Test]
        public void Office_Post_Should_Return_Bad_Request_If_Model_State_Is_Not_Valid()
        {
            _officeController.ModelState.AddModelError("key", "error message");
            var result = _officeController.Post(null);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
        }

        [Test]
        public void Office_Post_Should_Return_Conflict_Message_If()
        {
            var result = _officeController.Post(null);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
        }

        [Test]
        public void Office_Put_Should_Return_Default_Office_When_It_Was_Provided_2()
        {
            var previousDefaultOffice = _officeController.GetDefault();
            var newDefaultOffice = new Office
            {
                Id = 28,
                Name = "NewDefaultOffice",
                IsDefault = true
            };

            var newDefaultOfficePostModel = _mapper.Map<Office, OfficePostViewModel>(newDefaultOffice);
            var result = _officeController.Post(newDefaultOfficePostModel);
            var changedDefaultOffice = _officeController.GetDefault();

            Assert.AreNotEqual(previousDefaultOffice.Id, changedDefaultOffice.Id);
        }
    }
}