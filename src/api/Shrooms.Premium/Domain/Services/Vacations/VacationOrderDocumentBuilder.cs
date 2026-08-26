using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
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
    /// What it says lives in <see cref="VacationOrderContent"/>, shared with the
    /// .pdf rendering of the same order.
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

                foreach (var line in VacationOrderContent.LetterheadLines(settings))
                {
                    body.AppendChild(Centered(line, bold: true));
                }

                body.AppendChild(Centered(string.Empty, bold: true));
                body.AppendChild(Centered(string.Empty, bold: true));
                body.AppendChild(Centered(string.Empty, bold: true));

                body.AppendChild(Centered(VacationOrderContent.Heading, bold: true));
                body.AppendChild(Centered(VacationOrderContent.TitleFor(order), bold: true));
                body.AppendChild(Centered(
                    VacationOrderContent.ReferenceLine(order),
                    bold: false,
                    size: ListSize));

                if (!string.IsNullOrWhiteSpace(settings.OrderCity))
                {
                    body.AppendChild(Centered(settings.OrderCity, bold: false));
                }

                body.AppendChild(Centered(string.Empty, bold: false));
                body.AppendChild(Centered(string.Empty, bold: false));

                body.AppendChild(BodyParagraph(VacationOrderContent.Preamble));
                body.AppendChild(BodyParagraph(string.Empty));

                foreach (var group in VacationOrderContent.Groups(order))
                {
                    body.AppendChild(BodyParagraph(VacationOrderContent.DecreeFor(group.Key)));

                    foreach (var item in VacationOrderContent.Lines(group))
                    {
                        body.AppendChild(BulletParagraph(VacationOrderContent.ItemLine(item)));
                    }

                    body.AppendChild(BodyParagraph(string.Empty));
                }

                if (VacationOrderContent.HasPayout(order))
                {
                    body.AppendChild(BodyParagraph(VacationOrderContent.Payout));
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
