using System.IO;
using System.Text;
using NUnit.Framework;
using Shrooms.Premium.Domain.Services.Vacations;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class VacationReportEncodingTests
    {
        private const string Report =
            "name;dateFrom;dateTo;type\n" +
            "Miglė Lukoševičiūtė;2026-08-24;2026-09-07;A\n";

        [Test]
        public void ToBytes_StartsWithTheUtf16LittleEndianBom()
        {
            var content = VacationReportEncoding.ToBytes(Report);

            // Without these two bytes the spreadsheet falls back to the local ANSI
            // code page and every Lithuanian name comes out as mojibake.
            Assert.That(content[0], Is.EqualTo(0xFF));
            Assert.That(content[1], Is.EqualTo(0xFE));
        }

        [Test]
        public void ToBytes_SurvivesTheImportReader()
        {
            var content = VacationReportEncoding.ToBytes(Report);

            // The same reader ImportAsync uses: UTF-8 by default, BOM wins.
            using var reader = new StreamReader(
                new MemoryStream(content),
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true);

            Assert.That(reader.ReadToEnd(), Is.EqualTo(Report));
        }

        [Test]
        public void ToBytes_WritesTheBomForAnEmptyReport()
        {
            var content = VacationReportEncoding.ToBytes(string.Empty);

            Assert.That(content, Is.EqualTo(new byte[] { 0xFF, 0xFE }));
        }
    }
}
