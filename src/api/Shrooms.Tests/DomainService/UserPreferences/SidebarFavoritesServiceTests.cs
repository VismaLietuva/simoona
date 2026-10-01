using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.UserPreferences;
using Shrooms.Domain.Services.Wall.Widgets;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService.UserPreferences
{
    [TestFixture]
    public class SidebarFavoritesServiceTests
    {
        private const string UserId = "user-1";
        private const int OrganizationId = 2;

        private IUnitOfWork2 _uow;
        private ApplicationUser _user;
        private SidebarFavoritesService _sut;
        private WallWidgetPreferencesService _widgets;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _user = new ApplicationUser { Id = UserId, OrganizationId = OrganizationId };
            _uow.MockDbSetForAsync(new List<ApplicationUser>
            {
                _user,
                new ApplicationUser { Id = "someone-else", OrganizationId = OrganizationId, WallWidgetPreferences = "{\"sidebarFavorites\":[\"kudos\"]}" }
            });

            _sut = new SidebarFavoritesService(_uow);
            _widgets = new WallWidgetPreferencesService(_uow);
        }

        [Test]
        public async Task GetAsync_NeverSaved_ReturnsNull()
        {
            Assert.That(await _sut.GetAsync(UserOrg()), Is.Null);
        }

        [Test]
        public async Task GetAsync_LegacyWidgetArrayStored_ReturnsNull()
        {
            _user.WallWidgetPreferences = "[{\"id\":\"polls\",\"visible\":true}]";

            Assert.That(await _sut.GetAsync(UserOrg()), Is.Null);
        }

        [Test]
        public async Task SaveAsync_KeepsTheWidgetLayout()
        {
            _user.WallWidgetPreferences = "[{\"id\":\"polls\",\"visible\":true}]";

            await _sut.SaveAsync("[\"events\",\"kudos\"]", UserOrg());

            Assert.That(await _sut.GetAsync(UserOrg()), Is.EqualTo("[\"events\",\"kudos\"]"));
            Assert.That(await _widgets.GetAsync(UserOrg()), Is.EqualTo("[{\"id\":\"polls\",\"visible\":true}]"));
            await _uow.Received(1).SaveChangesAsync(UserId);
        }

        [Test]
        public async Task WidgetSave_KeepsTheFavorites()
        {
            await _sut.SaveAsync("[\"events\"]", UserOrg());

            await _widgets.SaveAsync("[{\"id\":\"birthdays\",\"visible\":false}]", UserOrg());

            Assert.That(await _sut.GetAsync(UserOrg()), Is.EqualTo("[\"events\"]"));
            Assert.That(await _widgets.GetAsync(UserOrg()), Is.EqualTo("[{\"id\":\"birthdays\",\"visible\":false}]"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("{not json")]
        [TestCase("{\"events\":true}")]
        [TestCase("[\"events\",42]")]
        public void SaveAsync_NotAnArrayOfStrings_Throws(string favorites)
        {
            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _sut.SaveAsync(favorites, UserOrg()));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.SidebarFavoritesInvalid));
            Assert.That(_user.WallWidgetPreferences, Is.Null);
        }

        [Test]
        public void SaveAsync_TooLong_Throws()
        {
            var favorites = "[\"" + new string('a', SidebarFavoritesService.MaxFavoritesLength) + "\"]";

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _sut.SaveAsync(favorites, UserOrg()));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.SidebarFavoritesInvalid));
        }

        [Test]
        public void SaveAsync_UserInAnotherOrganization_Throws()
        {
            var otherOrg = new UserAndOrganizationDto { UserId = UserId, OrganizationId = OrganizationId + 1 };

            var ex = Assert.ThrowsAsync<ValidationException>(async () => await _sut.SaveAsync("[\"events\"]", otherOrg));

            Assert.That(ex.ErrorCode, Is.EqualTo(ErrorCodes.UserNotFound));
        }

        private static UserAndOrganizationDto UserOrg()
        {
            return new UserAndOrganizationDto { UserId = UserId, OrganizationId = OrganizationId };
        }
    }
}
