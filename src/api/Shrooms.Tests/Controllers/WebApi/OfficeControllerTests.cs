using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.FilterPresets;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.WebViewModels.Models;
using Shrooms.Presentation.WebViewModels.Models.PostModels;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.Mocks;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.Controllers.WebApi
{
    [TestFixture]
    internal class OfficeControllerTests
    {
        private IUnitOfWork _unitOfWork;
        private IFilterPresetService _filterPresetService;
        private OfficeController _officeController;
        private IMapper _mapper;

        [SetUp]
        public void TestInitializer()
        {
            _unitOfWork = new MockUnitOfWork();
            _mapper = ModelMapper.Create();

            _filterPresetService = Substitute.For<IFilterPresetService>();

            _officeController = new OfficeController(_mapper, _unitOfWork, _filterPresetService);

            _officeController.SetUpControllerForTesting();
        }

        [Test]
        public async Task Office_GetAll_Should_Return_All_Offices()
        {
            var result = (await _officeController.GetAll()) as List<OfficeViewModel>;
            Assert.That(result, Is.Not.Null);
            Assert.That(4, Is.EqualTo(result.Count));
        }

        [Test]
        public async Task Office_Get_Returns_Correct_Office()
        {
            var result = await _officeController.Get(1);
            var model = result.GetContent<OfficeViewModel>();
            Assert.That("B-Office", Is.EqualTo(model.Name));
        }

        [Test]
        public async Task Office_Get_Should_Return_Bad_Request_If_Giving_Incorrect_Id()
        {
            var result = await _officeController.Get(0);
            Assert.That(result.GetStatusCode(), Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Office_GetDefault_Should_Return_View_Model()
        {
            var model = await _officeController.GetDefault();
            Assert.That(model, Is.InstanceOf<OfficeViewModel>());
        }

        [Test]
        public async Task Office_GetPaged_Should_Return_List_Of_All_Offices()
        {
            var result = await _officeController.GetPaged();
            Assert.That(result.PagedList.Count, Is.EqualTo(4));
        }

        [Test]
        [TestCase("City", WebApiConstants.DefaultPageSize, "A-Office")]
        [TestCase("StreetBuilding", WebApiConstants.DefaultPageSize, "B-Office")]
        public async Task Office_GetPaged_Should_Return_Sorted_List(string sort, int amountResult, string officeNameResult)
        {
            var result = await _officeController.GetPaged(sort: sort);
            Assert.That(officeNameResult, Is.EqualTo(result.PagedList.FirstOrDefault()?.Name));
        }

        [Test]
        public async Task Office_GetPaged_Should_Return_Searched_Offices()
        {
            var result = await _officeController.GetPaged(s: "B-Office");
            Assert.That(1, Is.EqualTo(result.PagedList.Count));
            Assert.That("B-Office", Is.EqualTo(result.PagedList.FirstOrDefault()?.Name));
        }

        [Test]
        public async Task Office_Put_Should_Return_Bad_Request_If_Model_State_Is_Not_Valid()
        {
            _officeController.ModelState.AddModelError("key", "error message");
            var result = await _officeController.Put(null);
            Assert.That(HttpStatusCode.BadRequest, Is.EqualTo(result.GetStatusCode()));
        }

        [Test]
        public async Task Office_Put_Should_Return_Bad_Request_If()
        {
            var result = await _officeController.Put(null);
            Assert.That(result.GetStatusCode(), Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Office_Delete_Should_Return_Not_Found_If_Office_Was_Deleted()
        {
            var result = await _officeController.Delete(default);
            Assert.That(HttpStatusCode.NotFound, Is.EqualTo(result.GetStatusCode()));
        }

        [Test]
        public async Task Office_Delete_Removes_Types_From_Presets()
        {
            // Arrange
            const int id = 1;

            // Act
            await _officeController.Delete(id);

            // Assert
            await _filterPresetService.Received(1)
                .RemoveDeletedTypeFromPresetsAsync(Arg.Is(id.ToString()), FilterType.Offices, Arg.Any<int>());
        }

        [Test]
        public async Task Office_Post_Should_Return_New_Office_And_Ok_Response_If_Added_Successfully()
        {
            var testOffice = new OfficePostViewModel
            {
                Name = "NewOffice"
            };

            var result = await _officeController.Post(testOffice);
            Assert.That(HttpStatusCode.Created, Is.EqualTo(result.GetStatusCode()));
        }

        [Test]
        public async Task Office_Post_Should_Return_Bad_Request_If_Model_State_Is_Not_Valid()
        {
            _officeController.ModelState.AddModelError("key", "error message");
            var result = await _officeController.Post(null);
            Assert.That(HttpStatusCode.BadRequest, Is.EqualTo(result.GetStatusCode()));
        }

        [Test]
        public async Task Office_Post_Should_Return_Conflict_Message_If()
        {
            var result = await _officeController.Post(null);
            Assert.That(HttpStatusCode.BadRequest, Is.EqualTo(result.GetStatusCode()));
        }

        [Test]
        public async Task Office_Put_Should_Return_Default_Office_When_It_Was_Provided_2()
        {
            var previousDefaultOffice = await _officeController.GetDefault();
            var newDefaultOffice = new Office
            {
                Id = 28,
                Name = "NewDefaultOffice",
                IsDefault = true
            };

            var newDefaultOfficePostModel = _mapper.Map<Office, OfficePostViewModel>(newDefaultOffice);
            await _officeController.Post(newDefaultOfficePostModel);
            var changedDefaultOffice = await _officeController.GetDefault();

            Assert.That(changedDefaultOffice.Id, Is.Not.EqualTo(previousDefaultOffice.Id));
        }
    }
}
