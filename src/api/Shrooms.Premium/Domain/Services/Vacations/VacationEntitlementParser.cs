using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ExcelDataReader;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public class EntitlementRow
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public double? Total { get; set; }

        public double? Used { get; set; }

        /// <summary>Closing balance ("Likutis pabaigai"). The one column the import needs.</summary>
        public double Unused { get; set; }
    }

    public class EntitlementParseResult
    {
        public IList<EntitlementRow> Rows { get; set; } = new List<EntitlementRow>();

        public int Unreadable { get; set; }

        /// <summary>Used when the administrator leaves the "as of" field empty.</summary>
        public DateTime? DetectedAsOf { get; set; }
    }

    /// <summary>
    /// The legacy Excel sheet is read by fixed column index; the CSV export of
    /// the same report is read by header name, because that file is not a clean
    /// table — it opens with a title, a page number and a filter description and
    /// closes with blank rows and a totals line. Anchoring on the header row is
    /// what makes the surrounding noise harmless.
    /// </summary>
    public static class VacationEntitlementParser
    {
        private const int CodeColIndex = 0;
        private const int FullNameColIndex = 1;
        private const int VacationTotalTimeColIndex = 6;
        private const int VacationUsedTimeColIndex = 7;
        private const int VacationUnusedTimeColIndex = 8;

        private static readonly char[] Separators = { ';', ',', '\t' };

        private static readonly string[] CodeHeaders = { "nr", "code", "kodas", "employeecode", "employeeid", "tabnr" };

        private static readonly string[] NameHeaders =
        {
            "vardaspavarde", "name", "fullname", "employee", "vardas", "darbuotojas", "lastnamefirstname", "pavarde"
        };

        /// <summary>Most specific first: "likutis" alone also matches the opening balance.</summary>
        private static readonly string[] UnusedHeaders =
        {
            "likutispabaigai", "nepanaudota", "unused", "unusedtime", "vacationunusedtime", "remaining", "balance", "entitlement", "days", "dienos"
        };

        private static readonly string[] TotalHeaders = { "sukaupta", "total", "totaltime", "accrued", "priskaiciuota" };

        private static readonly string[] UsedHeaders = { "panaudota", "used", "usedtime", "istaikyta" };

        private static readonly Regex IsoDate = new Regex(@"\b(\d{4}-\d{2}-\d{2})\b", RegexOptions.Compiled);

        public static EntitlementParseResult ParseCsv(string text)
        {
            var result = new EntitlementParseResult();

            var lines = (text ?? string.Empty)
                .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
                .ToList();

            if (lines.Count == 0)
            {
                return result;
            }

            var separator = PickSeparator(lines);
            var rows = lines.Select(line => SplitLine(line, separator)).ToList();

            result.DetectedAsOf = DetectDate(lines);

            var headerIndex = FindHeaderRow(rows);

            int codeAt;
            int nameAt;
            int unusedAt;
            var totalAt = -1;
            var usedAt = -1;

            if (headerIndex >= 0)
            {
                var headers = rows[headerIndex].Select(HeaderKey).ToList();
                codeAt = FindColumn(headers, CodeHeaders);
                nameAt = FindColumn(headers, NameHeaders);

                // Balance first, then excluded from the other two: "unused"
                // contains "used", so the loose pass would hand Used the balance
                // column — wrong in a way nothing downstream would notice.
                unusedAt = FindColumn(headers, UnusedHeaders);
                totalAt = FindColumn(headers, TotalHeaders, unusedAt);
                usedAt = FindColumn(headers, UsedHeaders, unusedAt, totalAt);
            }
            else
            {
                // Headerless export: code, name, days.
                headerIndex = -1;
                codeAt = 0;
                nameAt = 1;
                unusedAt = 2;
            }

            for (var i = headerIndex + 1; i < rows.Count; i++)
            {
                var cells = rows[i];

                // Spacer rows are structure, not failed entries; the export is
                // full of them and they would report a clean import as broken.
                if (cells.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                var name = At(cells, nameAt);
                var unused = ToNumber(At(cells, unusedAt));

                if (IsTotalsRow(name, At(cells, codeAt)))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(name) || unused == null)
                {
                    result.Unreadable++;
                    continue;
                }

                result.Rows.Add(new EntitlementRow
                {
                    Code = At(cells, codeAt),
                    Name = name,
                    Total = ToNumber(At(cells, totalAt)),
                    Used = ToNumber(At(cells, usedAt)),
                    Unused = unused.Value
                });
            }

            return result;
        }

        public static EntitlementParseResult ParseExcel(Stream stream)
        {
            var result = new EntitlementParseResult();

            using var reader = ExcelReaderFactory.CreateReader(stream);

            while (reader.Read())
            {
                if (reader.FieldCount <= VacationUnusedTimeColIndex)
                {
                    continue;
                }

                var code = reader.GetValue(CodeColIndex);
                var fullName = reader.GetValue(FullNameColIndex);
                var total = reader.GetValue(VacationTotalTimeColIndex);
                var used = reader.GetValue(VacationUsedTimeColIndex);
                var unused = reader.GetValue(VacationUnusedTimeColIndex);

                if (fullName is not string name || string.IsNullOrWhiteSpace(name) || !IsNumeric(unused))
                {
                    // Header and title rows land here, and are not failures.
                    continue;
                }

                result.Rows.Add(new EntitlementRow
                {
                    Code = code?.ToString(),
                    Name = name,
                    Total = IsNumeric(total) ? Convert.ToDouble(total) : null,
                    Used = IsNumeric(used) ? Convert.ToDouble(used) : null,
                    Unused = Convert.ToDouble(unused)
                });
            }

            return result;
        }

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var decomposed = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var ch in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                builder.Append(char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : ' ');
            }

            return Regex.Replace(builder.ToString(), @"\s+", " ").Trim();
        }

        private static string HeaderKey(string value)
        {
            return Normalize(value).Replace(" ", string.Empty);
        }

        /// <summary>
        /// Both a person column and a balance column, or the title row
        /// ("Atostogų ataskaita") is mistaken for the header.
        /// </summary>
        private static int FindHeaderRow(IReadOnlyList<string[]> rows)
        {
            for (var i = 0; i < rows.Count && i < 30; i++)
            {
                var headers = rows[i].Select(HeaderKey).ToList();
                if (FindColumn(headers, NameHeaders) >= 0 && FindColumn(headers, UnusedHeaders) >= 0)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Skips any column already claimed by another field.</summary>
        private static int FindColumn(IReadOnlyList<string> headers, IReadOnlyList<string> names, params int[] taken)
        {
            bool Available(int index) => index >= 0 && !taken.Contains(index);

            // Exact first, so "likutispabaigai" beats a looser alias elsewhere.
            foreach (var name in names)
            {
                var exact = FindIndex(headers, header => header == name, taken);
                if (Available(exact))
                {
                    return exact;
                }
            }

            foreach (var name in names)
            {
                var partial = FindIndex(headers, header => header.Length > 0 && header.Contains(name), taken);
                if (Available(partial))
                {
                    return partial;
                }
            }

            return -1;
        }

        private static int FindIndex(IReadOnlyList<string> headers, Func<string, bool> predicate, IReadOnlyCollection<int> taken)
        {
            for (var i = 0; i < headers.Count; i++)
            {
                if (!taken.Contains(i) && predicate(headers[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string At(IReadOnlyList<string> cells, int index)
        {
            return index >= 0 && index < cells.Count ? cells[index] : null;
        }

        /// <summary>The "Viso:" line parses cleanly and would import as an employee.</summary>
        private static bool IsTotalsRow(string name, string code)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var normalized = Normalize(name);
            return normalized is "viso" or "total" or "iš viso" or "is viso"
                   || (string.IsNullOrWhiteSpace(code) && name.TrimEnd().EndsWith(":", StringComparison.Ordinal));
        }

        private static char PickSeparator(IReadOnlyList<string> lines)
        {
            // Over several lines: the title row of a real export has none.
            var sample = lines.Take(10).ToList();

            return Separators
                .OrderByDescending(candidate => sample.Sum(line => line.Count(ch => ch == candidate)))
                .First();
        }

        private static string[] SplitLine(string line, char separator)
        {
            var cells = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];

                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (ch == separator && !inQuotes)
                {
                    cells.Add(current.ToString().Trim());
                    current.Clear();
                    continue;
                }

                current.Append(ch);
            }

            cells.Add(current.ToString().Trim());
            return cells.ToArray();
        }

        /// <summary>"2 865,78" and "2865.78" are the same number.</summary>
        private static double? ToNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var cleaned = value
                .Replace(" ", string.Empty)
                .Replace(" ", string.Empty);

            // Payroll writes "1,5"; a spreadsheet may write "1,234.50".
            // Whichever separator comes last is the decimal one, and the other
            // groups thousands.
            var comma = cleaned.LastIndexOf(',');
            var dot = cleaned.LastIndexOf('.');

            cleaned = comma > dot
                ? cleaned.Replace(".", string.Empty).Replace(',', '.')
                : cleaned.Replace(",", string.Empty);

            return double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        private static DateTime? DetectDate(IEnumerable<string> lines)
        {
            foreach (var line in lines.Take(10))
            {
                var match = IsoDate.Match(line ?? string.Empty);
                if (match.Success && DateTime.TryParseExact(
                        match.Groups[1].Value,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsed))
                {
                    return parsed.Date;
                }
            }

            return null;
        }

        private static bool IsNumeric(object value)
        {
            return value is double || value is int || value is decimal || value is float || value is long;
        }
    }
}
