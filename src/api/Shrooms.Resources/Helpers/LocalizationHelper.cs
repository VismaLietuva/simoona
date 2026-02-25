using System.Text.Json;

namespace Shrooms.Resources.Helpers
{
    public static class LocalizationHelper
    {
        public static string ToJson(this object item)
        {
            return JsonSerializer.Serialize(item);
        }
    }
}