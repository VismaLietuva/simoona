using System;
using System.IO;
using System.Linq;
using System.Reflection;
using PdfSharp.Fonts;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// Feeds PDFsharp the one typeface a leave order is set in. The API runs on
    /// the aspnet runtime image, which ships no fonts at all, so the face is
    /// embedded in this assembly rather than looked up on the host — and the
    /// order carries Lithuanian diacritics that PDF's built-in Times cannot
    /// encode, so a real Unicode font is not optional.
    ///
    /// Liberation Serif is metric-compatible with Times New Roman, so the .pdf
    /// breaks its lines where the .docx does.
    /// </summary>
    internal sealed class VacationOrderFonts : IFontResolver
    {
        public const string FamilyName = "Liberation Serif";

        private const string RegularFace = "LiberationSerif-Regular";
        private const string BoldFace = "LiberationSerif-Bold";

        private static readonly VacationOrderFonts Resolver = new();

        private static readonly object Gate = new();

        private static bool _registered;

        /// <summary>
        /// PDFsharp resolves fonts through one process-wide hook, and refuses a
        /// second one once a font has been built. Set it once, and leave a
        /// resolver somebody else installed alone.
        /// </summary>
        public static void Register()
        {
            if (_registered)
            {
                return;
            }

            lock (Gate)
            {
                if (_registered)
                {
                    return;
                }

                GlobalFontSettings.FontResolver ??= Resolver;
                _registered = true;
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo(isBold ? BoldFace : RegularFace);
        }

        public byte[] GetFont(string faceName)
        {
            var assembly = typeof(VacationOrderFonts).GetTypeInfo().Assembly;
            var suffix = $".{faceName}.ttf";

            var name = assembly.GetManifestResourceNames()
                .FirstOrDefault(candidate => candidate.EndsWith(suffix, StringComparison.Ordinal));

            if (name == null)
            {
                throw new InvalidOperationException($"Embedded font '{faceName}' is missing from {assembly.GetName().Name}.");
            }

            using var source = assembly.GetManifestResourceStream(name);
            using var buffer = new MemoryStream();
            source.CopyTo(buffer);

            return buffer.ToArray();
        }
    }
}
