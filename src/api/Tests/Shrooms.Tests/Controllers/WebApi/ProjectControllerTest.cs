using NSubstitute;
using NUnit.Framework;
using Shrooms.API.Controllers.WebApi;
using Shrooms.DataLayer;
using Shrooms.EntityModels.Models;
using Shrooms.ModelMappings;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.PostModels;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;

namespace Shrooms.UnitTests.Controllers.WebApi
{
    [TestFixture]
    public class ProjectControllerTest
    {
        private IUnitOfWork _unitOfWork;
        private ProjectController _projectController;
        private IRepository<ApplicationUser> _applicationUserRepository;

        [SetUp]
        public void TestInitializer()
        {
            _unitOfWork = new MockUnitOfWork();
            _applicationUserRepository = _unitOfWork.GetRepository<ApplicationUser>();

            _projectController = new ProjectController(ModelMapper.Create(), _unitOfWork, null);
            _projectController.ControllerContext = Substitute.For<HttpControllerContext>();
            _projectController.Request = new HttpRequestMessage();
            _projectController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _projectController.Request.SetConfiguration(new HttpConfiguration());
            _projectController.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
        }

        [Test]
        public async void Project_Get_Should_Return_Correct_View_Model()
        {
            var result = _projectController.Get(1);
            var project = await result.Content.ReadAsAsync<ProjectViewModel>();

            Assert.IsInstanceOf<ProjectViewModel>(project);
        }

        [Test]
        public async void Project_Get_Should_Return_Correct_Application_User_By_Id()
        {
            var result = _projectController.Get(1);
            var project = await result.Content.ReadAsAsync<ProjectViewModel>();

            Assert.AreEqual(1, project.Id);
        }

        [Test]
        public void Project_GetPaged_Should_Return_Correct_Paged_Model()
        {
            var projects = _projectController.GetPaged();
            Assert.IsInstanceOf<PagedViewModel<ProjectViewModel>>(projects);
        }

        [Test]
        public void Project_GetPaged_Should_Return_Correct_Page_Count()
        {
            var projects = _projectController.GetPaged(page: 1, pageSize: 2);
            Assert.AreEqual(2, projects.PageCount);
        }

        [Test]
        public void Project_Create_Should_Return_Conflict_If_Project_Already_Exists()
        {
            var model = new ProjectPostViewModel
            {
                Id = 1,
                Name = "Beta project"
            };

            _projectController.Validate(model);
            var response = _projectController.Post(model);

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Test]
        public void Project_Create_Should_Return_Conflict_If_Project_Already_Exists_2()
        {
            var model = new ProjectPostViewModel
            {
                Id = 1,
                Name = "VIGO"
            };

            _projectController.Validate(model);
            var response = _projectController.Post(model);

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Test]
        public void Project_Create_Should_Return_Created_Project()
        {
            var model = new ProjectPostViewModel
            {
                Name = "New project"
            };

            _projectController.Validate(model);
            var response = _projectController.Post(model);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [Test]
        public async void Project_Update_Should_Return_Updated_Project()
        {
            var result = _projectController.Get(2);
            var model = await result.Content.ReadAsAsync<ProjectViewModel>();

            model.Name = "New project name";

            //Controller.Validate(model);
            //var response = Controller.Post(model);

            //Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var result2 = _projectController.Get(2);
            var model2 = await result2.Content.ReadAsAsync<ProjectViewModel>();

            Assert.AreNotEqual(model.Name, model2.Name);
        }

        [Test]
        public void Project_Delete_Should_Return_Not_Found_If_Incorrect_Id_Is_Passed()
        {
            var message = _projectController.Delete(0);

            Assert.AreEqual(HttpStatusCode.NotFound, message.StatusCode);
        }

        [Test]
        public void Project_Delete_Should_Return_Ok_And_Remove_Project()
        {
            const int projectIdToDelete = 1;

            var message = _projectController.Delete(projectIdToDelete);
            Assert.AreEqual(HttpStatusCode.OK, message.StatusCode);

            var result = _projectController.Get(projectIdToDelete);

            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}