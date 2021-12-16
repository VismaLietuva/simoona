using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
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

            _organizationsDbSet = Substitute.For<DbSet<Organization>, IQueryable<Organization>, IDbAsyncEnumerable<Organization>>();
            _organizationsDbSet.SetDbSetDataForAsync(_mockDbContext.Organizations);
            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IDbAsyncEnumerable<ApplicationUser>>();
            _usersDbSet.SetDbSetDataForAsync(_mockDbContext.ApplicationUsers);

            var mockDbSqlQuery = Substitute.For<DbSqlQuery<ApplicationUser>, IDbAsyncEnumerable<ApplicationUser>>();
            var asyncEnumerator = new MockDbAsyncEnumerator<ApplicationUser>(_mockDbContext.ApplicationUsers.GetEnumerator());

            ((IDbAsyncEnumerable<ApplicationUser>)mockDbSqlQuery).GetAsyncEnumerator().Returns(asyncEnumerator);

            mockDbSqlQuery.AsNoTracking().Returns(mockDbSqlQuery);
            mockDbSqlQuery.GetEnumerator().Returns(_mockDbContext.ApplicationUsers.GetEnumerator());

            _usersDbSet.SqlQuery(Arg.Any<string>(), Arg.Any<object[]>())
                .Returns(mockDbSqlQuery);

            _uow = Substitute.For<IUnitOfWork2>();
            _uow.GetDbSet<ApplicationUser>().ReturnsForAnyArgs(_usersDbSet);
            _uow.GetDbSet<Organization>().ReturnsForAnyArgs(_organizationsDbSet);

            _pictureService = Substitute.For<IPictureService>();

            _usersAnonymizationWebHookService = new UsersAnonymizationWebHookService(_uow, _pictureService);
        }

        [Test]
        public async Task Should_Anonymize_All_Users()
        {
            // Arrange
            var organization = _mockDbContext.Organizations.First();

            // Act
            await _usersAnonymizationWebHookService.AnonymizeUsersAsync(organization.ShortName);

            // Assert
            Assert.IsFalse(_usersDbSet.Any(user => !user.IsAnonymized));
        }
    }
}