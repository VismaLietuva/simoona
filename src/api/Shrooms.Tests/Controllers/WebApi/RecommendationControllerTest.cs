using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Domain.Services.Recommendation;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Tests.ModelMappings;
using Shrooms.Presentation.WebViewModels.Models.Recommendation;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    public class RecommendationControllerTest
    {
        private RecommendationController _recommendationController;

        private IRecommendationService _recommendationService;

        [SetUp]
        public void testInitializer()
        {

            _recommendationService= Substitute.For<IRecommendationService>();
            _recommendationController = Substitute.For<RecommendationController>(ModelMapper.Create(), _recommendationService);
            _recommendationController.ControllerContext = Substitute.For<HttpControllerContext>();
            _recommendationController.Request = new HttpRequestMessage();
            _recommendationController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _recommendationController.Request.SetConfiguration(new HttpConfiguration());
            _recommendationController.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim("OrganizationId", "1") }));
        }
 
        [Test]
        public async Task SubmitRecommendation_Should_Return_Created()
        {
            var httpActionResult = await _recommendationController.SubmitTicket(new RecommendationPostViewModel()
            {
                Name = "name",
                Contact = "email",
                LastName = "lastname",
                Message = "message"
            });
                   
            Assert.AreEqual(HttpStatusCode.Created, httpActionResult.StatusCode);
        }
    }
}
