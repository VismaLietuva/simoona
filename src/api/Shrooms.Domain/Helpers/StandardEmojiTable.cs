using System.Text.Json;

namespace Shrooms.Domain.Helpers
{
    public static class StandardEmojiTable
    {
        private const string ResourceName = "Shrooms.Domain.emoji-shortcodes.json";

        private static readonly Lazy<Dictionary<string, string[]>> Table = new(Load);

        public static int Count => Table.Value.Count;

        public static bool TryGet(string name, out IReadOnlyList<string> variants)
        {
            var found = Table.Value.TryGetValue(name, out var value);
            variants = value;
            return found;
        }

        private static Dictionary<string, string[]> Load()
        {
            using var stream = typeof(StandardEmojiTable).Assembly.GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException($"Embedded resource {ResourceName} is missing.");
            using var document = JsonDocument.Parse(stream);

            var root = document.RootElement;
            var table = new Dictionary<string, string[]>(StringComparer.Ordinal);

            foreach (var entry in root.GetProperty("emojis").EnumerateObject())
            {
                table[entry.Name] = entry.Value.ValueKind == JsonValueKind.Array
                    ? entry.Value.EnumerateArray().Select(variant => variant.GetString()).ToArray()
                    : new[] { entry.Value.GetString() };
            }

            var expected = root.GetProperty("count").GetInt32();
            if (table.Count != expected)
            {
                throw new InvalidOperationException($"{ResourceName} lists {table.Count} emoji but its header says {expected}.");
            }

            return table;
        }
    }
}
