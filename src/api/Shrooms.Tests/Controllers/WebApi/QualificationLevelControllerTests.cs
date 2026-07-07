using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
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
    public class QualificationLevelControllerTests
    {
        private IUnitOfWork _unitOfWork;
        private QualificationLevelController _qualificationLevelController;

        [SetUp]
        public void TestInitializer()
        {
            _unitOfWork = new MockUnitOfWork();

            _qualificationLevelController = new QualificationLevelController(ModelMapper.Create(), _unitOfWork);
            _qualificationLevelController.SetUpControllerForTesting();
        }

        [Test]
        public async Task QualificationLevel_Get_Should_Return_Correct_Id()
        {
            var result = await _qualificationLevelController.Get(1);
            var model = result.GetContent<QualificationLevelViewModel>();

            Assert.That(model.Id, Is.EqualTo(1));
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

            Assert.That(response.GetStatusCode(), Is.EqualTo(HttpStatusCode.Created));
        }
    }
}
