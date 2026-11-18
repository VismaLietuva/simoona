using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Shrooms.Presentation.Api.Helpers
{
    /// <summary>
    /// The Newtonsoft settings every API response is written with. Program.cs applies them to MVC, and tests that
    /// check what a response contains apply them too, so they see the JSON a client gets.
    /// </summary>
    public static class ApiJsonSettings
    {
        public static void Apply(JsonSerializerSettings settings)
        {
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.Converters.Add(new EmptyToNullConverter());
            settings.Converters.Add(new FormattedDecimalConverter());
        }

        public static JsonSerializerSettings Create()
        {
            var settings = new JsonSerializerSettings();
            Apply(settings);
            return settings;
        }
    }
}
