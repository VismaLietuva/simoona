using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// Renders a leave order as a .docx, reproducing the signed paper it replaces
    /// down to its typography: Times New Roman throughout, bold 12pt for the
    /// letterhead and the two title lines, 11pt for the reference line and the
    /// bulleted list, 12pt for everything else, 18pt of leading, and a page of
    /// US Letter with one-inch margins.
    ///
    /// The Lithuanian wording is literal, not translated: this is a legal
    /// document that goes to payroll, so it is a constant here for the same
    /// reason the Vacation Bot letters are constants on the client.
    /// </summary>
    internal static class VacationOrderDocumentBuilder
    {
        private const string FontName = "Times New Roman";

        private const string BodySize = "24";

        private const string ListSize = "22";

        /// <summary>Twips. 360 = 18pt of leading, exactly, on every line.</summary>
        private const string LineSpacing = "360";

        private const string BodyIndent = "900";

        private const string ListIndent = "1260";
        private const string ListHanging = "360";

        private const int BulletNumberId = 1;

        private static readonly string[] MonthsGenitive =
        {
            "sausio", "vasario", "kovo", "balandžio", "gegužės", "birželio",
            "liepos", "rugpjūčio", "rugsėjo", "spalio", "lapkričio", "gruodžio"
        };

        public static byte[] Build(VacationOrder order, VacationSettingsDto settings)
        {
            using var stream = new MemoryStream();

            using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
            {
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();
                AddBulletDefinition(mainPart);
                AddSettings(mainPart);

                var body = mainPart.Document.AppendChild(new Body());

                foreach (var line in LetterheadLines(settings))
                {
                    body.AppendChild(Centered(line, bold: true));
                }

                body.AppendChild(Centered(string.Empty, bold: true));
                body.AppendChild(Centered(string.Empty, bold: true));
                body.AppendChild(Centered(string.Empty, bold: true));

                body.AppendChild(Centered("ĮSAKYMAS ", bold: true));
                body.AppendChild(Centered(TitleFor(order), bold: true));
                body.AppendChild(Centered(
                    $"{VacationWireFormat.ToDay(order.IssuedOn)} d. Nr.{order.Reference}",
                    bold: false,
                    size: ListSize));

                if (!string.IsNullOrWhiteSpace(settings.OrderCity))
                {
                    body.AppendChild(Centered(settings.OrderCity, bold: false));
                }

                body.AppendChild(Centered(string.Empty, bold: false));
                body.AppendChild(Centered(string.Empty, bold: false));

                body.AppendChild(BodyParagraph("Atsižvelgdamas į prašymus,"));
                body.AppendChild(BodyParagraph(string.Empty));

                // Each leave type gets its own decree sentence: one order can
                // grant annual and unpaid leave, and the wording differs.
                foreach (var group in order.Items.GroupBy(item => item.Type).OrderBy(group => group.Key))
                {
                    body.AppendChild(BodyParagraph(DecreeFor(group.Key)));

                    foreach (var item in group.OrderBy(item => item.EmployeeName, StringComparer.CurrentCulture))
                    {
                        body.AppendChild(BulletParagraph(ItemLine(item)));
                    }

                    body.AppendChild(BodyParagraph(string.Empty));
                }

                // Only paid leave is paid out: the unpaid and parental orders end
                // at the list, as the signed ones do.
                if (order.Items.Any(item => item.Type == VacationRequestType.Annual))
                {
                    body.AppendChild(BodyParagraph("Išmokant priskaičiuotus atostoginius su mėnesio atlyginimu."));
                }

                body.AppendChild(BodyParagraph(string.Empty, JustificationValues.Left));
                body.AppendChild(BodyParagraph(string.Empty, JustificationValues.Left));

                if (!string.IsNullOrWhiteSpace(settings.OrderSignature))
                {
                    body.AppendChild(SignatureParagraph(settings.OrderSignature));
                }

                body.AppendChild(Trailing());
                body.AppendChild(Trailing());
                body.AppendChild(PageSetup());

                mainPart.Document.Save();
            }

            return stream.ToArray();
        }

        private static void AddBulletDefinition(MainDocumentPart mainPart)
        {
            var part = mainPart.AddNewPart<NumberingDefinitionsPart>();

            var level = new Level(
                new StartNumberingValue { Val = 1 },
                new NumberingFormat { Val = NumberFormatValues.Bullet },
                // U+F0B7 is the private-use codepoint Word writes for Symbol's bullet.
                new LevelText { Val = "" },
                new LevelJustification { Val = LevelJustificationValues.Left },
                new PreviousParagraphProperties(new Indentation { Left = "0", Hanging = "0" }),
                new NumberingSymbolRunProperties(new RunFonts
                {
                    Ascii = "Symbol",
                    HighAnsi = "Symbol",
                    ComplexScript = "Symbol",
                    Hint = FontTypeHintValues.Default
                }))
            {
                LevelIndex = 0
            };

            part.Numbering = new Numbering(
                new AbstractNum(level) { AbstractNumberId = BulletNumberId },
                new NumberingInstance(new AbstractNumId { Val = BulletNumberId }) { NumberID = BulletNumberId });

            part.Numbering.Save();
        }

        private static void AddSettings(MainDocumentPart mainPart)
        {
            var part = mainPart.AddNewPart<DocumentSettingsPart>();
            part.Settings = new Settings(new DefaultTabStop { Val = 709 });
            part.Settings.Save();
        }

        private static SectionProperties PageSetup()
        {
            return new SectionProperties(
                new PageSize { Width = 12240, Height = 15840 },
                new PageMargin
                {
                    Top = 1440,
                    Right = 1440,
                    Bottom = 1440,
                    Left = 1440,
                    Header = 0,
                    Footer = 0,
                    Gutter = 0
                });
        }

        private static IEnumerable<string> LetterheadLines(VacationSettingsDto settings)
        {
            return (settings.OrderLetterhead ?? string.Empty)
                .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0);
        }

        private static string TitleFor(VacationOrder order)
        {
            var types = order.Items.Select(item => item.Type).Distinct().ToList();
            if (types.Count != 1)
            {
                return "DĖL ATOSTOGŲ SUTEIKIMO";
            }

            return types[0] switch
            {
                VacationRequestType.Annual => "DĖL KASMETINIŲ ATOSTOGŲ SUTEIKIMO",
                VacationRequestType.Unpaid => "DĖL NEMOKAMŲ ATOSTOGŲ SUTEIKIMO",
                VacationRequestType.Parental => "DĖL PAPILDOMOS POILSIO DIENOS SUTEIKIMO",
                _ => "DĖL ATOSTOGŲ SUTEIKIMO"
            };
        }

        private static string DecreeFor(VacationRequestType type)
        {
            // The spaced "Į s a k a u" is how the original document sets the
            // operative verb; reproduced literally so the two look alike.
            var what = type switch
            {
                VacationRequestType.Annual => "kasmetines apmokamas atostogas",
                VacationRequestType.Unpaid => "nemokamas atostogas",
                VacationRequestType.Parental => "papildomas poilsio dienas (tėvadienius)",
                _ => "atostogas"
            };

            return $"Į s a k a u  suteikti {what} šiems darbuotojams:";
        }

        private static string ItemLine(VacationOrderItem item)
        {
            // A single day is written out long-hand and a period as a range, which
            // is the distinction the original document draws.
            return item.DateFrom.Date == item.DateTo.Date
                ? $"{item.EmployeeName} : {LongDate(item.DateFrom)} imtinai;"
                : $"{item.EmployeeName} : nuo {VacationWireFormat.ToDay(item.DateFrom)} d. iki {VacationWireFormat.ToDay(item.DateTo)} d. imtinai;";
        }

        private static string LongDate(DateTime date)
        {
            return $"{date.Year} m. {MonthsGenitive[date.Month - 1]} {date.Day} d.";
        }

        private static Paragraph Centered(string text, bool bold, string size = BodySize)
        {
            return Paragraph(
                text,
                Properties(JustificationValues.Center, new Indentation { Left = "0", Right = "0", Hanging = "0" }),
                bold,
                size);
        }

        private static Paragraph BodyParagraph(string text, JustificationValues? justification = null)
        {
            return Paragraph(
                text,
                Properties(
                    justification ?? JustificationValues.Both,
                    new Indentation { Left = "0", Right = "0", FirstLine = BodyIndent }),
                bold: false,
                size: BodySize);
        }

        private static Paragraph BulletParagraph(string text)
        {
            var properties = Properties(
                JustificationValues.Both,
                new Indentation { Left = ListIndent, Right = "0", Hanging = ListHanging });

            properties.PrependChild(new NumberingProperties(
                new NumberingLevelReference { Val = 0 },
                new NumberingId { Val = BulletNumberId }));

            return Paragraph(text, properties, bold: false, size: ListSize);
        }

        private static Paragraph Trailing()
        {
            return Paragraph(
                string.Empty,
                new ParagraphProperties(
                    new SpacingBetweenLines { Before = "0", After = "0", Line = "240", LineRule = LineSpacingRuleValues.Exact },
                    new Indentation { Left = "0", Right = "0", Hanging = "0" },
                    new Justification { Val = JustificationValues.Left }),
                bold: false,
                size: BodySize);
        }

        private static Paragraph SignatureParagraph(string signature)
        {
            // Title on the left, name across the page — a tab in the source
            // string, rendered as five tabs so the two line up as they do on
            // paper.
            var parts = signature.Split('\t');
            var paragraph = new Paragraph(Properties(
                JustificationValues.Both,
                new Indentation { Left = "0", Right = "0", FirstLine = BodyIndent }));

            var run = new Run(RunProperties(bold: false, size: BodySize));

            for (var i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    for (var tab = 0; tab < 5; tab++)
                    {
                        run.AppendChild(new TabChar());
                    }
                }

                run.AppendChild(new Text(parts[i]) { Space = SpaceProcessingModeValues.Preserve });
            }

            paragraph.AppendChild(run);
            return paragraph;
        }

        private static Paragraph Paragraph(string text, ParagraphProperties properties, bool bold, string size)
        {
            var paragraph = new Paragraph(properties);
            var run = new Run(RunProperties(bold, size));

            // Space="preserve", or Word collapses the spacing that "Į s a k a u"
            // and the trailing space after "ĮSAKYMAS" depend on.
            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            paragraph.AppendChild(run);

            return paragraph;
        }

        private static ParagraphProperties Properties(JustificationValues justification, Indentation indentation)
        {
            return new ParagraphProperties(
                new SpacingBetweenLines
                {
                    Before = "0",
                    After = "0",
                    Line = LineSpacing,
                    LineRule = LineSpacingRuleValues.Exact
                },
                indentation,
                new Justification { Val = justification });
        }

        private static RunProperties RunProperties(bool bold, string size)
        {
            var properties = new RunProperties(
                new RunFonts
                {
                    Ascii = FontName,
                    HighAnsi = FontName,
                    EastAsia = FontName,
                    ComplexScript = FontName
                },
                new Color { Val = "000000" },
                new FontSize { Val = size });

            if (bold)
            {
                properties.PrependChild(new Bold());
            }

            return properties;
        }
    }
}
