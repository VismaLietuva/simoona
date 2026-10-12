using System.Text.Json;
using System.Text.Json.Nodes;

namespace Shrooms.Domain.Services.UserPreferences
{
    /// <summary>
    /// ApplicationUser.WallWidgetPreferences holds every per-user UI preference
    /// as one JSON object with a section per feature:
    /// <c>{"widgets":[...],"sidebarFavorites":[...]}</c>.
    /// The column started out as the bare widget array, which is still read as
    /// the widgets section. Each feature reads and writes only its own section,
    /// so saving one never overwrites another.
    /// </summary>
    public static class UserPreferencesDocument
    {
        public const string WidgetsSection = "widgets";
        public const string SidebarFavoritesSection = "sidebarFavorites";

        /// <summary>
        /// Returns the section as a JSON string, or null when it was never saved.
        /// </summary>
        public static string GetSection(string stored, string section)
        {
            return Parse(stored)[section]?.ToJsonString();
        }

        /// <summary>
        /// Replaces one section and keeps every other one as stored.
        /// <paramref name="value"/> must already be validated JSON.
        /// </summary>
        public static string SetSection(string stored, string section, string value)
        {
            var document = Parse(stored);
            document[section] = JsonNode.Parse(value);
            return document.ToJsonString();
        }

        // Anything unreadable is treated as empty, so a corrupt value heals on the
        // next save instead of failing every request.
        private static JsonObject Parse(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored))
            {
                return new JsonObject();
            }

            JsonNode node;

            try
            {
                node = JsonNode.Parse(stored);
            }
            catch (JsonException)
            {
                return new JsonObject();
            }

            return node switch
            {
                JsonObject document => document,
                JsonArray widgets => new JsonObject { [WidgetsSection] = widgets },
                _ => new JsonObject()
            };
        }
    }
}
