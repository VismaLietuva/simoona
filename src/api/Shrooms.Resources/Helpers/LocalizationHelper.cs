namespace Shrooms.Resources.Helpers
{
    using System.Text.Json;

    public static class LocalizationHelper
    {
        public static string ToJson(this object item)
        {
            return JsonSerializer.Serialize(item);
        }
    }
}
