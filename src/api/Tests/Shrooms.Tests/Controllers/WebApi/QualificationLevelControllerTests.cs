using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using NSubstitute;
using NUnit.Framework;
using Shrooms.API.Controllers.WebApi;
using Shrooms.DataLayer;
using Shrooms.ModelMappings;
using Shrooms.UnitTests.ModelMappings;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.PostModels;

namespace Shrooms.UnitTests.Controllers.WebApi
{
    [TestFixture]
    public class QualificationLevelControllerTests
    {
        private IUnitOfWork _unitOfWork;
        private QualificationLevelController _qualificationLevelController;

        [SetUp]
        public void TestInitializer()
        {
            _unitOfWork = new MockUnitOfWork();

            _qualificationLevelController = new QualificationLevelController(ModelMapper.Create(), _unitOfWork);
            _qualificationLevelController.ControllerContext = Substitute.For<HttpControllerContext>();
            _qualificationLevelController.Request = new HttpRequestMessage();
            _qualificationLevelController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _qualificationLevelController.Request.SetConfiguration(new HttpConfiguration());
            _qualificationLevelController.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
        }

        [Test]
        public async void QualificationLevel_Get_Should_Return_Correct_Id()
        {
            var result = _qualificationLevelController.Get(1);
            var model = await result.Content.ReadAsAsync<QualificationLevelViewModel>();

            Assert.AreEqual(1, model.Id);
        }

        [Test]
        public void QualificationLevel_Post_Should_Return_Created_Entity_If_Saved_Successfully()
        {
            QualificationLevelPostViewModel model = new QualificationLevelPostViewModel();
            model.Id = 0;
            model.Name = "test";
            model.SortOrder = 0;
            _qualificationLevelController.Validate(model);
            HttpResponseMessage response = _qualificationLevelController.Post(model);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }
    }
}