using System.Linq;
using NUnit.Framework;
using Shrooms.Contracts.Enums;
using Shrooms.Premium.Domain.Services.Vacations;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class VacationReportParserTests
    {
        /// <summary>What the export writes, BOM and all.</summary>
        private const string RealReport =
            "﻿name;dateFrom;dateTo;type\n" +
            "Arturas Test;2026-08-03;2026-08-07;A\n" +
            "Rūta Trumpauskaitė;2026-08-10;2026-08-10;M\n";

        [Test]
        public void ParseCsv_ReadsTheExportedShape()
        {
            var result = VacationReportParser.ParseCsv(RealReport);

            Assert.That(result.Rows.Count, Is.EqualTo(2));
            Assert.That(result.UnreadableLines, Is.Empty);

            var first = result.Rows.First();
            Assert.That(first.Line, Is.EqualTo(2));
            Assert.That(first.Name, Is.EqualTo("Arturas Test"));
            Assert.That(first.DateFrom, Is.EqualTo("2026-08-03"));
            Assert.That(first.DateTo, Is.EqualTo("2026-08-07"));
            Assert.That(first.Type, Is.EqualTo("A"));

            Assert.That(result.Rows.Last().Name, Is.EqualTo("Rūta Trumpauskaitė"));
        }

        [Test]
        public void ParseCsv_KeepsTheFirstRowWhenItIsNotAHeader()
        {
            var result = VacationReportParser.ParseCsv("Arturas Test;2026-08-03;2026-08-07;A\n");

            Assert.That(result.Rows.Count, Is.EqualTo(1));
            Assert.That(result.Rows.Single().Line, Is.EqualTo(1));
        }

        [Test]
        public void ParseCsv_UnquotesANameCarryingTheSeparator()
        {
            var result = VacationReportParser.ParseCsv("\"Test, Arturas\";2026-08-03;2026-08-07;A\n");

            Assert.That(result.Rows.Single().Name, Is.EqualTo("Test, Arturas"));
        }

        /// <summary>The export prefixes an apostrophe so a spreadsheet cannot evaluate the cell.</summary>
        [Test]
        public void ParseCsv_DropsTheFormulaGuard()
        {
            var result = VacationReportParser.ParseCsv("'-Arturas Test;2026-08-03;2026-08-07;A\n");

            Assert.That(result.Rows.Single().Name, Is.EqualTo("-Arturas Test"));
        }

        [Test]
        public void ParseCsv_ReportsALineWithTooFewFields()
        {
            var result = VacationReportParser.ParseCsv(
                "name;dateFrom;dateTo;type\nArturas Test;2026-08-03\n\nArturas Test;2026-08-10;2026-08-14;A\n");

            Assert.That(result.UnreadableLines, Is.EqualTo(new[] { 2 }));
            Assert.That(result.Rows.Count, Is.EqualTo(1));
        }

        [Test]
        public void ParseCsv_ReadsACommaSeparatedFile()
        {
            var result = VacationReportParser.ParseCsv("name,dateFrom,dateTo,type\nArturas Test,2026-08-03,2026-08-07,A\n");

            Assert.That(result.Rows.Single().DateTo, Is.EqualTo("2026-08-07"));
        }

        /// <summary>Payroll's own codes: A atostogos, M tėvadienis, NA nemokamos atostogos.</summary>
        [TestCase(VacationRequestType.Annual, "A")]
        [TestCase(VacationRequestType.Parental, "M")]
        [TestCase(VacationRequestType.Unpaid, "NA")]
        public void ReportLetter_RoundTripsPayrollsCodes(VacationRequestType type, string code)
        {
            Assert.That(VacationWireFormat.TypeToReportLetter(type), Is.EqualTo(code));
            Assert.That(VacationWireFormat.ParseReportLetter(code), Is.EqualTo(type));
            Assert.That(VacationWireFormat.ParseReportLetter(code.ToLowerInvariant()), Is.EqualTo(type));
        }

        [TestCase("T")]
        [TestCase("N")]
        [TestCase("X")]
        [TestCase("")]
        public void ReportLetter_RefusesACodePayrollDoesNotUse(string code)
        {
            Assert.That(VacationWireFormat.ParseReportLetter(code), Is.Null);
        }

        /// <summary>A file re-saved by a spreadsheet can open with a blank line.</summary>
        [Test]
        public void ParseCsv_SkipsTheHeaderAfterALeadingBlankLine()
        {
            var result = VacationReportParser.ParseCsv(
                "\nname;dateFrom;dateTo;type\nArturas Test;2026-08-03;2026-08-07;A\n");

            Assert.That(result.Rows.Count, Is.EqualTo(1));
            Assert.That(result.Rows.Single().Name, Is.EqualTo("Arturas Test"));
            Assert.That(result.UnreadableLines, Is.Empty);
        }

        [Test]
        public void ParseCsv_IsEmptyForAHeaderOnlyFile()
        {
            var result = VacationReportParser.ParseCsv("name;dateFrom;dateTo;type\n");

            Assert.That(result.Rows, Is.Empty);
            Assert.That(result.UnreadableLines, Is.Empty);
        }
    }
}
