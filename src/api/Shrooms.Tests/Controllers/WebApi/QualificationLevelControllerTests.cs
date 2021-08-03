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
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using Shrooms.Tests.Mocks;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.Controllers.WebApi
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
        public async Task QualificationLevel_Get_Should_Return_Correct_Id()
        {
            var result = await _qualificationLevelController.Get(1);
            var model = await result.Content.ReadAsAsync<QualificationLevelViewModel>();

            Assert.AreEqual(1, model.Id);
        }

        [Test]
        public async Task QualificationLevel_Post_Should_Return_Created_Entity_If_Saved_Successfully()
        {
            var model = new QualificationLevelPostViewModel
            {
                Id = 0,
                Name = "test",
                SortOrder = 0
            };

            _qualificationLevelController.Validate(model);
            var response = await _qualificationLevelController.Post(model);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }
    }
}
