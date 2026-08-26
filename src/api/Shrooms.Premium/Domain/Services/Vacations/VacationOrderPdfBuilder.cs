using System;
using System.Collections.Generic;
using System.IO;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// Renders the same leave order as a .pdf: the page the .docx describes,
    /// already laid out. Payroll files the Word copy, but a signatory only wants
    /// to read and print one, and a .pdf looks the same everywhere.
    ///
    /// The measurements are the .docx measurements converted from twips to
    /// points — US Letter, one-inch margins, 18pt of leading, 12pt body and 11pt
    /// list — set in Liberation Serif, which is metric-compatible with Times New
    /// Roman, so the two break their lines in the same places.
    /// </summary>
    internal static class VacationOrderPdfBuilder
    {
        private const double PageWidth = 612;
        private const double PageHeight = 792;
        private const double Margin = 72;
        private const double ContentWidth = PageWidth - (2 * Margin);

        private const double LineHeight = 18;
        private const double BodySize = 12;
        private const double ListSize = 11;

        /// <summary>900 twips of first-line indent, in points.</summary>
        private const double BodyIndent = 45;

        private const double ListIndent = 63;
        private const double ListHanging = 18;

        /// <summary>The .docx sets its default tab stop at 709 twips.</summary>
        private const double TabStop = 35.45;

        public static byte[] Build(VacationOrder order, VacationSettingsDto settings)
        {
            VacationOrderFonts.Register();

            var body = new XFont(VacationOrderFonts.FamilyName, BodySize);
            var bold = new XFont(VacationOrderFonts.FamilyName, BodySize, XFontStyleEx.Bold);
            var list = new XFont(VacationOrderFonts.FamilyName, ListSize);

            using var document = new PdfDocument();

            using (var page = new Page(document))
            {
                foreach (var line in VacationOrderContent.LetterheadLines(settings))
                {
                    page.Centered(line, bold);
                }

                page.Blank();
                page.Blank();
                page.Blank();

                page.Centered(VacationOrderContent.Heading, bold);
                page.Centered(VacationOrderContent.TitleFor(order), bold);
                page.Centered(VacationOrderContent.ReferenceLine(order), list);

                if (!string.IsNullOrWhiteSpace(settings.OrderCity))
                {
                    page.Centered(settings.OrderCity, body);
                }

                page.Blank();
                page.Blank();

                page.Justified(VacationOrderContent.Preamble, body);
                page.Blank();

                foreach (var group in VacationOrderContent.Groups(order))
                {
                    page.Justified(VacationOrderContent.DecreeFor(group.Key), body);

                    foreach (var item in VacationOrderContent.Lines(group))
                    {
                        page.Bullet(VacationOrderContent.ItemLine(item), list);
                    }

                    page.Blank();
                }

                if (VacationOrderContent.HasPayout(order))
                {
                    page.Justified(VacationOrderContent.Payout, body);
                }

                page.Blank();
                page.Blank();

                if (!string.IsNullOrWhiteSpace(settings.OrderSignature))
                {
                    page.Signature(settings.OrderSignature, body);
                }
            }

            using var stream = new MemoryStream();
            document.Save(stream, closeStream: false);

            return stream.ToArray();
        }

        /// <summary>
        /// A cursor down the page. Every line sits exactly <see cref="LineHeight"/>
        /// below the one above it, and running past the bottom margin starts a new
        /// sheet — an order granting leave to forty people is a longer document,
        /// not a truncated one.
        /// </summary>
        private sealed class Page : IDisposable
        {
            private readonly PdfDocument _document;

            private XGraphics _gfx;

            private double _y;

            public Page(PdfDocument document)
            {
                _document = document;
                Start();
            }

            public void Blank()
            {
                Advance();
            }

            public void Centered(string text, XFont font)
            {
                foreach (var line in WrapTokens(text, font, ContentWidth, 0))
                {
                    var joined = string.Join(" ", line);
                    Room();
                    DrawAt(joined, font, Margin + ((ContentWidth - Width(joined, font)) / 2), _y);
                    Advance();
                }
            }

            /// <summary>
            /// Justified to both margins, as the .docx is: every line but the last
            /// of a paragraph shares its leftover width out between the gaps.
            /// </summary>
            public void Justified(string text, XFont font)
            {
                if (string.IsNullOrEmpty(text))
                {
                    Blank();
                    return;
                }

                var lines = WrapTokens(text, font, ContentWidth, BodyIndent);

                for (var i = 0; i < lines.Count; i++)
                {
                    var indent = i == 0 ? BodyIndent : 0;
                    var last = i == lines.Count - 1;

                    DrawTokens(lines[i], font, Margin + indent, ContentWidth - indent, justify: !last);
                }
            }

            /// <summary>
            /// The hanging-indent list: the bullet sits at the body indent and the
            /// text, including every wrapped line, at the list indent.
            /// </summary>
            public void Bullet(string text, XFont font)
            {
                var lines = WrapTokens(text, font, ContentWidth - ListIndent, 0);

                for (var i = 0; i < lines.Count; i++)
                {
                    Room();

                    if (i == 0)
                    {
                        DrawAt("•", font, Margin + ListIndent - ListHanging, _y);
                    }

                    var last = i == lines.Count - 1;
                    DrawTokens(lines[i], font, Margin + ListIndent, ContentWidth - ListIndent, justify: !last);
                }
            }

            /// <summary>
            /// Title on the left, name across the page — a tab in the stored
            /// signature, rendered as five tab stops so the two line up as they do
            /// on paper.
            /// </summary>
            public void Signature(string signature, XFont font)
            {
                var parts = signature.Split('\t');
                var x = Margin + BodyIndent;

                Room();

                for (var i = 0; i < parts.Length; i++)
                {
                    if (i > 0)
                    {
                        for (var tab = 0; tab < 5; tab++)
                        {
                            x = NextTabStop(x);
                        }
                    }

                    DrawAt(parts[i], font, x, _y);
                    x += Width(parts[i], font);
                }

                Advance();
            }

            public void Dispose()
            {
                _gfx?.Dispose();
                _gfx = null;
            }

            private static double NextTabStop(double x)
            {
                var stops = Math.Floor(((x - Margin) / TabStop) + 1e-9) + 1;
                return Margin + (stops * TabStop);
            }

            private void Start()
            {
                _gfx?.Dispose();

                var page = _document.AddPage();
                page.Size = PageSize.Letter;

                _gfx = XGraphics.FromPdfPage(page);
                _y = Margin;
            }

            private void Room()
            {
                if (_y + LineHeight > PageHeight - Margin)
                {
                    Start();
                }
            }

            private void Advance()
            {
                _y += LineHeight;
            }

            private void DrawAt(string text, XFont font, double x, double y)
            {
                if (text.Length > 0)
                {
                    _gfx.DrawString(text, font, XBrushes.Black, x, y, XStringFormats.TopLeft);
                }
            }

            private void DrawTokens(IList<string> tokens, XFont font, double x, double available, bool justify)
            {
                Room();

                var space = Width(" ", font);
                var gaps = tokens.Count - 1;
                var extra = 0d;

                if (justify && gaps > 0)
                {
                    var used = space * gaps;

                    foreach (var token in tokens)
                    {
                        used += Width(token, font);
                    }

                    extra = Math.Max(0, available - used) / gaps;
                }

                var cursor = x;

                foreach (var token in tokens)
                {
                    DrawAt(token, font, cursor, _y);
                    cursor += Width(token, font) + space + extra;
                }

                Advance();
            }

            /// <summary>
            /// Split on single spaces and kept as tokens, so the double space in
            /// the decree sentence survives as an empty token and stays a double
            /// space on the page.
            /// </summary>
            private List<List<string>> WrapTokens(string text, XFont font, double available, double firstIndent)
            {
                var lines = new List<List<string>>();
                var current = new List<string>();
                var width = 0d;
                var space = Width(" ", font);
                var room = available - firstIndent;

                foreach (var token in text.Split(' '))
                {
                    var size = Width(token, font);
                    var added = current.Count == 0 ? size : width + space + size;

                    if (current.Count > 0 && added > room)
                    {
                        lines.Add(current);
                        current = new List<string> { token };
                        width = size;
                        room = available;
                        continue;
                    }

                    current.Add(token);
                    width = added;
                }

                lines.Add(current);
                return lines;
            }

            private double Width(string text, XFont font)
            {
                return text.Length == 0 ? 0 : _gfx.MeasureString(text, font).Width;
            }
        }
    }
}
