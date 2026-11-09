using System.Globalization;
using System.Linq;
using System.Text;

namespace Shrooms.DataLayer.DAL.Search
{
    /// <summary>
    /// SearchText is stored lower-case with accents removed ("Žukauskas" → "zukauskas")
    /// and compared in a binary collation, which is several times faster than an
    /// accent-insensitive one. The same table folds the stored text (in SQL) and the
    /// typed words (in C#), so the two always agree.
    /// </summary>
    public static class SearchTextNormalizer
    {
        public const int MaxLength = 4000;

        public const string Collation = "Latin1_General_100_BIN2";

        private static readonly (string From, string To) Fold = BuildFold();

        /// <summary>A T-SQL expression that folds <paramref name="expression"/> like <see cref="Normalize"/>.</summary>
        public static string Sql(string expression) =>
            $"TRANSLATE(LOWER(LEFT({expression}, {MaxLength})), N'{Fold.From}', N'{Fold.To}') COLLATE {Collation}";

        public static string Normalize(string text)
        {
            var lower = text.ToLowerInvariant();
            var builder = new StringBuilder(lower.Length);
            foreach (var character in lower)
            {
                var index = Fold.From.IndexOf(character);
                builder.Append(index < 0 ? character : Fold.To[index]);
            }

            return builder.ToString();
        }

        // Every lower-case Latin letter in U+00E0–U+017F that decomposes to a plain ASCII
        // letter plus marks (ą, č, ė, š, ū, ž, é, ö, ...), and the few that do not
        // decompose but read as one (ø, ł, đ, ħ, ŧ, ı).
        private static (string From, string To) BuildFold()
        {
            var from = new StringBuilder("øłđħŧı");
            var to = new StringBuilder("oldhti");
            for (var code = 0x00E0; code <= 0x017F; code++)
            {
                var character = (char)code;
                if (!char.IsLower(character))
                {
                    continue;
                }

                var baseLetter = character.ToString()
                    .Normalize(NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray();
                if (baseLetter.Length == 1 && baseLetter[0] is >= 'a' and <= 'z' && baseLetter[0] != character)
                {
                    from.Append(character);
                    to.Append(baseLetter[0]);
                }
            }

            return (from.ToString(), to.ToString());
        }
    }
}
