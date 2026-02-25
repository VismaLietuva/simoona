using AutoMapper;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Kudos;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.ViewModels;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Presentation.Common.Controllers.Kudos;
using Shrooms.Presentation.WebViewModels.Models.Users.Kudos;
using Microsoft.AspNetCore.Mvc;
using Shrooms.Tests.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shrooms.Tests.Controllers.WebApi
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
            _kudosController.SetUpControllerForTesting();
        }

        [Test]
        public async Task GetKudosTypes_Should_Return_IEnumerable_Of_KudosType_ViewModel()
        {
            var userAndOrganization = new UserAndOrganizationDto
            {
                OrganizationId = 1,
                UserId = "fd798623-166c-412d-a060-369d4c7b90eb"
            };

            _kudosService.GetKudosTypesAsync(userAndOrganization).Returns(new List<KudosTypeDto> { new() });

            var response = await _kudosController.GetKudosTypes();

            Assert.That(response, Is.InstanceOf<IEnumerable<KudosTypeViewModel>>());
        }

        [Test]
        public async Task Kudos_GetKudosLog_Should_Return_View_Model()
        {
            // Arrange
            var filter = new KudosLogsFilterViewModel
            {
                Page = 1,
                SearchUserId = "1"
            };

            IEnumerable<MainKudosLogDto> kudosLogs = new List<MainKudosLogDto>
            {
                new()
                {
                    Id = 1
                }
            };

            var entries = new KudosLogsEntriesDto<MainKudosLogDto>
            {
                KudosLogs = kudosLogs
            };

            var dto = new KudosLogsFilterDto();

            _mapper.Map<KudosLogsFilterViewModel, KudosLogsFilterDto>(filter)
                .Returns(dto);

            _kudosService.GetKudosLogsAsync(null).ReturnsForAnyArgs(entries);

            // Act
            var response = await _kudosController.GetKudosLogs(filter);

            // Assert
            Assert.That(response, Is.InstanceOf<PagedViewModel<KudosLogViewModel>>());
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

            var mappedRequest = new AddKudosLogDto();
            _mapper.Map<AddKudosLogViewModel, AddKudosLogDto>(request).Returns(mappedRequest);
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Kudos)
                .Returns(true);

            // Act
            var result = await _kudosController.AddKudosLog(request);

            // Assert
            Assert.That(result, Is.InstanceOf<OkResult>());
            await _kudosService.Received(1).AddKudosLogAsync(mappedRequest, explicitAmount);
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
            var mappedRequest = new AddKudosLogDto();
            _mapper.Map<AddKudosLogViewModel, AddKudosLogDto>(request).Returns(mappedRequest);
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Kudos)
                .Returns(true);

            // Act
            var result = await _kudosController.AddKudosLog(request);

            // Assert
            Assert.That(result, Is.InstanceOf<OkResult>());
            await _kudosService.DidNotReceive().AddKudosLogAsync(mappedRequest, explicitAmount);
            await _kudosService.Received(1).AddKudosLogAsync(mappedRequest);
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

            var mappedRequest = new AddKudosLogDto();
            _mapper.Map<AddKudosLogViewModel, AddKudosLogDto>(request).Returns(mappedRequest);
            _permissionService.UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), AdministrationPermissions.Kudos)
                .Returns(false);

            // Act
            var result = await _kudosController.AddKudosLog(request);

            // Assert
            Assert.That(result, Is.InstanceOf<OkResult>());
            await _kudosService.DidNotReceive().AddKudosLogAsync(mappedRequest, explicitAmount);
            await _kudosService.Received(1).AddKudosLogAsync(mappedRequest);
        }
    }
}
