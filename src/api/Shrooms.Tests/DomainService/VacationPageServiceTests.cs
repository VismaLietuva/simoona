using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.VacationPages;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.VacationPages;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.Mocks;
using Shrooms.Tests.ModelMappings;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class VacationPageServiceTests
    {
        private VacationPageService _vacationPageService;

        private DbSet<VacationPage> _vacationPagesDbSet;
        private MockDbContext _mockDbContext;
        private IUnitOfWork2 _uow;

        [SetUp]
        public void TestInitializer()
        {
            _mockDbContext = new MockDbContext();

            _vacationPagesDbSet = Substitute.For<DbSet<VacationPage>, IQueryable<VacationPage>, IDbAsyncEnumerable<VacationPage>>();
            _vacationPagesDbSet.SetDbSetDataForAsync(_mockDbContext.VacationPages);

            _uow = Substitute.For<IUnitOfWork2>();
            _uow.GetDbSet<VacationPage>().ReturnsForAnyArgs(_vacationPagesDbSet);

            _vacationPageService = new VacationPageService(_uow, ModelMapper.Create());
        }

        [Test]
        public async Task Should_Get_Vacation_Page()
        {
            // Act
            var vacationPageDto = await _vacationPageService.GetVacationPage(TestConstants.DefaultOrganizationId);

            // Assert
            Assert.NotNull(vacationPageDto);
        }

        [Test]
        public async Task Should_Return_Null_When_VacationPage_Does_Not_Exist()
        {
            // Act
            var vacationPageDto = await _vacationPageService.GetVacationPage(int.MaxValue);

            // Assert
            Assert.IsNull(vacationPageDto);
        }

        [Test]
        public async Task Should_Create_New_VacationPage_If_It_Does_Not_Exist()
        {
            // Arrange
            var vacationPageToAdd = new VacationPageDto
            {
                Content = "test"
            };

            var organizationId = int.MaxValue;

            var userAndOrg = new UserAndOrganizationDto
            {
                UserId = Guid.NewGuid().ToString(),
                OrganizationId = organizationId
            };

            // Act
            await _vacationPageService.EditVacationPage(userAndOrg, vacationPageToAdd);

            // Assert
            _vacationPagesDbSet.Received().Add(Arg.Any<VacationPage>());
        }

        [Test]
        public async Task Should_Update_VacationPage()
        {
            // Arrange
            var vacationPageUpdate = new VacationPageDto
            {
                Content = "update content"
            };

            var userAndOrg = new UserAndOrganizationDto
            {
                UserId = Guid.NewGuid().ToString(),
                OrganizationId = TestConstants.DefaultOrganizationId
            };

            // Act
            await _vacationPageService.EditVacationPage(userAndOrg, vacationPageUpdate);
            var actualUpdatedVacation = await _vacationPagesDbSet.FirstAsync(page => page.OrganizationId == TestConstants.DefaultOrganizationId);

            // Assert
            Assert.AreEqual(TestConstants.DefaultOrganizationId, actualUpdatedVacation.OrganizationId);
            Assert.AreEqual(vacationPageUpdate.Content, actualUpdatedVacation.Content);
        }
    }
}