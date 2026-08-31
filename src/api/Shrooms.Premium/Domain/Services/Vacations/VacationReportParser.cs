using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public class ReportRow
    {
        public int Line { get; set; }

        public string Name { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public string Type { get; set; }
    }

    public class ReportParseResult
    {
        public IList<ReportRow> Rows { get; set; } = new List<ReportRow>();

        public IList<int> UnreadableLines { get; set; } = new List<int>();
    }

    /// <summary>
    /// Reads back what <see cref="VacationReportService"/> writes:
    /// <c>name;dateFrom;dateTo;type</c>, one approved leave period per line. The
    /// header is optional so a hand-trimmed file still imports.
    /// </summary>
    public static class VacationReportParser
    {
        private static readonly char[] Separators = { ';', ',', '\t' };

        private static readonly string[] NameHeaders = { "name", "vardas", "vardaspavarde", "darbuotojas" };

        public static ReportParseResult ParseCsv(string text)
        {
            var result = new ReportParseResult();

            var lines = (text ?? string.Empty).Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            var separator = DetectSeparator(lines);

            var first = true;

            for (var index = 0; index < lines.Length; index++)
            {
                var line = lines[index];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var fields = SplitLine(line, separator);

                // The first line with anything on it, not line 1: a file re-saved
                // by a spreadsheet can open with a blank one.
                var header = first && IsHeader(fields);
                first = false;
                if (header)
                {
                    continue;
                }

                if (fields.Count < 4 || fields.Take(4).All(string.IsNullOrWhiteSpace))
                {
                    result.UnreadableLines.Add(index + 1);
                    continue;
                }

                result.Rows.Add(new ReportRow
                {
                    Line = index + 1,
                    Name = Clean(fields[0]),
                    DateFrom = Clean(fields[1]),
                    DateTo = Clean(fields[2]),
                    Type = Clean(fields[3])
                });
            }

            return result;
        }

        private static char DetectSeparator(IReadOnlyList<string> lines)
        {
            var sample = lines.Where(line => !string.IsNullOrWhiteSpace(line)).Take(10).ToList();

            return Separators
                .OrderByDescending(candidate => sample.Count(line => SplitLine(line, candidate).Count >= 4))
                .First();
        }

        private static bool IsHeader(IReadOnlyList<string> fields)
        {
            return fields.Count > 0 && NameHeaders.Contains(Normalize(Clean(fields[0])));
        }

        private static string Normalize(string value)
        {
            return new string((value ?? string.Empty)
                .Where(character => !char.IsWhiteSpace(character) && character != ',')
                .ToArray())
                .ToLowerInvariant();
        }

        /// <summary>
        /// Undoes the export's own quoting, and the apostrophe it prefixes to a
        /// value a spreadsheet would otherwise evaluate as a formula.
        /// </summary>
        private static string Clean(string value)
        {
            var text = (value ?? string.Empty).Trim().Trim('﻿');

            if (text.Length > 1 && text[0] == '\'')
            {
                text = text.Substring(1);
            }

            return text.Trim();
        }

        private static List<string> SplitLine(string line, char separator)
        {
            var fields = new List<string>();
            var field = new StringBuilder();
            var quoted = false;

            for (var index = 0; index < line.Length; index++)
            {
                var character = line[index];

                if (quoted)
                {
                    if (character != '"')
                    {
                        field.Append(character);
                        continue;
                    }

                    // A doubled quote inside a quoted field is one literal quote.
                    if (index + 1 < line.Length && line[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                        continue;
                    }

                    quoted = false;
                    continue;
                }

                if (character == '"' && field.Length == 0)
                {
                    quoted = true;
                    continue;
                }

                if (character == separator)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    continue;
                }

                field.Append(character);
            }

            fields.Add(field.ToString());
            return fields;
        }
    }
}
