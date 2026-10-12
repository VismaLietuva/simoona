using NUnit.Framework;
using Shrooms.Domain.Services.UserPreferences;

namespace Shrooms.Tests.DomainService.UserPreferences
{
    [TestFixture]
    public class UserPreferencesDocumentTests
    {
        private const string Widgets = UserPreferencesDocument.WidgetsSection;
        private const string Favorites = UserPreferencesDocument.SidebarFavoritesSection;

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("{not json")]
        [TestCase("42")]
        public void GetSection_NothingReadableStored_ReturnsNull(string stored)
        {
            Assert.That(UserPreferencesDocument.GetSection(stored, Widgets), Is.Null);
        }

        [Test]
        public void GetSection_LegacyBareArray_IsTheWidgetsSection()
        {
            const string stored = "[{\"id\":\"polls\",\"visible\":true}]";

            Assert.That(UserPreferencesDocument.GetSection(stored, Widgets), Is.EqualTo(stored));
            Assert.That(UserPreferencesDocument.GetSection(stored, Favorites), Is.Null);
        }

        [Test]
        public void GetSection_Document_ReturnsOnlyTheRequestedSection()
        {
            const string stored = "{\"widgets\":[{\"id\":\"polls\",\"visible\":false}],\"sidebarFavorites\":[\"events\"]}";

            Assert.That(UserPreferencesDocument.GetSection(stored, Widgets), Is.EqualTo("[{\"id\":\"polls\",\"visible\":false}]"));
            Assert.That(UserPreferencesDocument.GetSection(stored, Favorites), Is.EqualTo("[\"events\"]"));
        }

        [Test]
        public void SetSection_NothingStored_StartsADocument()
        {
            var result = UserPreferencesDocument.SetSection(null, Favorites, "[\"events\"]");

            Assert.That(result, Is.EqualTo("{\"sidebarFavorites\":[\"events\"]}"));
        }

        [Test]
        public void SetSection_LegacyBareArray_KeepsTheWidgets()
        {
            var result = UserPreferencesDocument.SetSection("[{\"id\":\"polls\",\"visible\":true}]", Favorites, "[\"events\"]");

            Assert.That(result, Is.EqualTo("{\"widgets\":[{\"id\":\"polls\",\"visible\":true}],\"sidebarFavorites\":[\"events\"]}"));
        }

        [Test]
        public void SetSection_ReplacesOnlyItsOwnSection()
        {
            const string stored = "{\"widgets\":[{\"id\":\"polls\",\"visible\":true}],\"sidebarFavorites\":[\"events\"]}";

            var result = UserPreferencesDocument.SetSection(stored, Widgets, "[{\"id\":\"birthdays\",\"visible\":false}]");

            Assert.That(result, Is.EqualTo("{\"widgets\":[{\"id\":\"birthdays\",\"visible\":false}],\"sidebarFavorites\":[\"events\"]}"));
        }

        [Test]
        public void SetSection_CorruptStoredValue_IsReplacedByANewDocument()
        {
            var result = UserPreferencesDocument.SetSection("{not json", Widgets, "[]");

            Assert.That(result, Is.EqualTo("{\"widgets\":[]}"));
        }
    }
}
