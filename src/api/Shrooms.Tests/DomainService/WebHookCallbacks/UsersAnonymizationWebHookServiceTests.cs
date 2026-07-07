using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Picture;
using Shrooms.Domain.Services.WebHookCallbacks.UserAnonymization;
using Shrooms.Tests.Extensions;
using Shrooms.Tests.Mocks;

namespace Shrooms.Tests.DomainService.WebHookCallbacks
{
    [TestFixture]
    public class UsersAnonymizationWebHookServiceTests
    {
        private UsersAnonymizationWebHookService _usersAnonymizationWebHookService;

        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<Organization> _organizationsDbSet;
        private IPictureService _pictureService;
        private MockDbContext _mockDbContext;
        private IUnitOfWork2 _uow;

        [SetUp]
        public void TestInitializer()
        {
            _mockDbContext = new MockDbContext();

            _organizationsDbSet = Substitute.For<DbSet<Organization>, IQueryable<Organization>, IAsyncEnumerable<Organization>>();
            _organizationsDbSet.SetDbSetDataForAsync(_mockDbContext.Organizations);
            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IAsyncEnumerable<ApplicationUser>>();
            _usersDbSet.SetDbSetDataForAsync(_mockDbContext.ApplicationUsers);

            _uow = Substitute.For<IUnitOfWork2>();
            _uow.GetDbSet<ApplicationUser>().ReturnsForAnyArgs(_usersDbSet);
            _uow.GetDbSet<Organization>().ReturnsForAnyArgs(_organizationsDbSet);

            _pictureService = Substitute.For<IPictureService>();

            var configuration = Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();
            _usersAnonymizationWebHookService = new UsersAnonymizationWebHookService(_uow, _pictureService, configuration);
        }

        [Test]
        public async Task Should_Anonymize_All_Users()
        {
            // Arrange
            var organization = _mockDbContext.Organizations.First();

            var deletedUsers = new List<ApplicationUser>
            {
                new()
                {
                    Id = "d1",
                    OrganizationId = organization.Id,
                    IsDeleted = true,
                    IsAnonymized = false,
                    Modified = DateTime.UtcNow.AddDays(-30)
                },
                new()
                {
                    Id = "d2",
                    OrganizationId = organization.Id,
                    IsDeleted = true,
                    IsAnonymized = false,
                    Modified = DateTime.UtcNow.AddDays(-30)
                }
            };

            _usersDbSet.SetDbSetDataForAsync(deletedUsers);

            // Act
            await _usersAnonymizationWebHookService.AnonymizeUsersAsync(organization.ShortName);

            // Assert
            Assert.That(_usersDbSet.Any(user => !user.IsAnonymized), Is.False);
        }
    }
}