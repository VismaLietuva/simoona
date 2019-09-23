using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using AutoMapper;
using NSubstitute;
using NUnit.Framework;
using Shrooms.API.Controllers.Kudos;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.Permissions;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.Kudos;

namespace Shrooms.UnitTests.Controllers.WebApi
{
    [TestFixture]
    public class KudosControllerTests
    {
        private KudosController _kudosController;

        private IMapper _mapper;
        private IKudosService _kudosService;
        private IKudosExportService _kudosExportService;
        private IPermissionService _permissionService;

        [SetUp]
        public void TestInitializer()
        {
            _mapper = Substitute.For<IMapper>();
            _kudosService = Substitute.For<IKudosService>();
            _kudosExportService = Substitute.For<IKudosExportService>();
            _permissionService = Substitute.For<IPermissionService>();

            _kudosController = new KudosController(_mapper, _kudosService, _kudosExportService, _permissionService);

            _kudosController.ControllerContext = Substitute.For<HttpControllerContext>();
            _kudosController.Request = new HttpRequestMessage();
            _kudosController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _kudosController.Request.SetConfiguration(new HttpConfiguration());
            _kudosController.RequestContext.Principal =
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim("OrganizationId", "1") }));
        }

        [Test]
        public void GetKodosTypes_Should_Return_IEnumerable_Of_KudosType_ViewModel()
        {
            var userAndOrganization = new UserAndOrganizationDTO()
            {
                OrganizationId = 1,
                UserId = "fd798623-166c-412d-a060-369d4c7b90eb"
            };

            _kudosService.GetKudosTypes(userAndOrganization)
                .Returns(new List<KudosTypeDTO>() { new KudosTypeDTO() });

            var response = _kudosController.GetKudosTypes();

            Assert.IsInstanceOf<IEnumerable<KudosTypeViewModel>>(response);
        }

        [Test]
        public void Kudos_GetKudosLog_Should_Return_View_Model()
        {
            // Arrange
            var filter = new KudosLogsFilterViewModel
            {
                Page = 1,
                SearchUserId = "1"
            };

            IEnumerable<MainKudosLogDTO> kudosLogs = new List<MainKudosLogDTO>
            {
                new MainKudosLogDTO
                {
                    Id = 1
                }
            };

            var entries = new KudosLogsEntriesDto<MainKudosLogDTO>
            {
                KudosLogs = kudosLogs
            };

            var dto = new KudosLogsFilterDTO();

            _mapper.Map<KudosLogsFilterViewModel, KudosLogsFilterDTO>(filter)
                .Returns(dto);

            _kudosService.GetKudosLogs(null).ReturnsForAnyArgs(entries);

            // Act
            var response = _kudosController.GetKudosLogs(filter);

            // Assert
            Assert.IsInstanceOf<PagedViewModel<KudosLogViewModel>>(response);
        }

        [Test]
        public async Task AddKudosLog_UserIsAdminAndTotalPointsIsNotNull_AddKudosLogWithExplicitAmountIsInvoked()
        {
            // Arrange
            const int explicitAmount = 123456;
            var request = new AddKudosLogViewModel
            {
                TotalPointsPerReceiver = explicitAmount
            };

            var mappedRequest = new AddKudosLogDTO();
            _mapper.Map<AddKudosLogViewModel, AddKudosLogDTO>(request).Returns(mappedRequest);
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDTO>(), AdministrationPermissions.Kudos)
                .Returns(true);

            // Act
            var result = await _kudosController.AddKudosLog(request).ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            _kudosService.Received(1).AddKudosLog(mappedRequest, explicitAmount);
        }

        [Test]
        public async Task AddKudosLog_UserIsAdminButTotalPointsIsNull_AddKudosLogWithExplicitAmountIsInvoked()
        {
            // Arrange
            const int explicitAmount = 123456;
            var request = new AddKudosLogViewModel
            {
                TotalPointsPerReceiver = null
            };
            var mappedRequest = new AddKudosLogDTO();
            _mapper.Map<AddKudosLogViewModel, AddKudosLogDTO>(request).Returns(mappedRequest);
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDTO>(), AdministrationPermissions.Kudos)
                .Returns(true);

            // Act
            var result = await _kudosController.AddKudosLog(request).ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            _kudosService.DidNotReceive().AddKudosLog(mappedRequest, explicitAmount);
            _kudosService.Received(1).AddKudosLog(mappedRequest);
        }

        [Test]
        public async Task AddKudosLog_UserIsNotAdminAndTotalPointsIsNotNull_AddKudosLogInvoked()
        {
            // Arrange
            const int explicitAmount = 123456;
            var request = new AddKudosLogViewModel
            {
                TotalPointsPerReceiver = explicitAmount
            };

            var mappedRequest = new AddKudosLogDTO();
            _mapper.Map<AddKudosLogViewModel, AddKudosLogDTO>(request).Returns(mappedRequest);
            _permissionService.UserHasPermission(Arg.Any<UserAndOrganizationDTO>(), AdministrationPermissions.Kudos)
                .Returns(false);

            // Act
            var result = await _kudosController.AddKudosLog(request).ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            _kudosService.DidNotReceive().AddKudosLog(mappedRequest, explicitAmount);
            _kudosService.Received(1).AddKudosLog(mappedRequest);
        }
    }
}
