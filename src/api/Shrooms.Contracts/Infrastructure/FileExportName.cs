using System;
using System.Text;

namespace Shrooms.Contracts.Infrastructure
{
    public static class FileExportName
    {
        private const int MaxBaseNameLength = 200;

        // Superset of characters disallowed by Windows/macOS/Linux filesystems and browser download UIs.
        private static readonly char[] Invalid = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

        public static string Sanitize(string desired, string fallback, string extension)
        {
            var baseName = SanitizeBase(desired);
            if (string.IsNullOrEmpty(baseName))
            {
                baseName = SanitizeBase(fallback);
            }

            if (baseName.Length > MaxBaseNameLength)
            {
                baseName = baseName.Substring(0, MaxBaseNameLength).TrimEnd();
            }

            return baseName + NormalizeExtension(extension);
        }

        private static string SanitizeBase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                if (char.IsControl(ch) || Array.IndexOf(Invalid, ch) >= 0)
                {
                    builder.Append('_');
                }
                else
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString().Trim().TrimEnd('.').Trim();
        }

        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return string.Empty;
            }

            return extension.StartsWith('.') ? extension : "." + extension;
        }
    }
}
