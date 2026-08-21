using System;
using System.Linq;
using NUnit.Framework;
using Shrooms.Premium.Domain.Services.Vacations;

namespace Shrooms.Premium.Tests.DomainService.VacationService
{
    [TestFixture]
    public class VacationEntitlementParserTests
    {
        /// <summary>
        /// The real payroll export, shape and all: a title row, a page number, a
        /// filter description in a quoted cell, blank spacer rows, and a closing
        /// totals line. None of that is a header or an employee.
        /// </summary>
        private const string RealExport =
            "Atostogų ataskaita;;;;;;;;2026-08-20;;;\n" +
            "Visma Tech, UAB;;;;;;;;Lapas;;;1\n" +
            ";;;;;;;;;;;\n" +
            "\"Datos filtras: 26-01-01..26-08-31; Nepan. atost. skaičiuoti pagal: Darbo dienos;\";;;;;;;;RAMINTA.PAKALNIENE;;;\n" +
            ";;;;;;;;;;;\n" +
            "Nr.;Vardas, pavardė;Pareigų aprašas;Likutis pradžiai;;Sukaupta;Panaudota;Likutis pabaigai;;;;\n" +
            ";;;;;;;;;;;\n" +
            "2011;Raminta Pakalnienė;Finansų vadovė;12,87;;13,68;21;5,55;;;;\n" +
            "2013;Ovidijus Varna;IT sistemų administratorius/-ė;14,99;;13,31;8;20,3;;;;\n" +
            ";;;;;;;;;;;\n" +
            ";Viso:;;2 865,78;;3 370,36;2 684,00;3 552,14;;;;\n";

        [Test]
        public void ParseCsv_ReadsTheRealExportWithoutMisreadingItsFurniture()
        {
            var result = VacationEntitlementParser.ParseCsv(RealExport);

            Assert.That(result.Rows.Count, Is.EqualTo(2));
            Assert.That(result.Unreadable, Is.EqualTo(0));
        }

        [Test]
        public void ParseCsv_TakesTheAsOfDateFromTheExportsOwnPreamble()
        {
            var result = VacationEntitlementParser.ParseCsv(RealExport);

            Assert.That(result.DetectedAsOf, Is.EqualTo(new DateTime(2026, 8, 20)));
        }

        [Test]
        public void ParseCsv_MapsTheClosingBalanceToUnusedAndNotTheOpeningOne()
        {
            var row = VacationEntitlementParser.ParseCsv(RealExport).Rows.First();

            Assert.That(row.Code, Is.EqualTo("2011"));
            Assert.That(row.Name, Is.EqualTo("Raminta Pakalnienė"));

            // "Likutis pabaigai", not "Likutis pradžiai" (12.87).
            Assert.That(row.Unused, Is.EqualTo(5.55));
        }

        [Test]
        public void ParseCsv_KeepsUsedAndUnusedApart()
        {
            var row = VacationEntitlementParser.ParseCsv(RealExport).Rows.First();

            // "unused" contains "used", so a loose header match once gave both
            // fields the same column — silently overwriting the used figure.
            Assert.That(row.Total, Is.EqualTo(13.68));
            Assert.That(row.Used, Is.EqualTo(21));
            Assert.That(row.Unused, Is.EqualTo(5.55));
        }

        [Test]
        public void ParseCsv_SkipsTheTotalsRow()
        {
            var names = VacationEntitlementParser.ParseCsv(RealExport).Rows.Select(row => row.Name);

            Assert.That(names, Does.Not.Contain("Viso:"));
        }

        [Test]
        public void ParseCsv_AcceptsBothDecimalSeparatorsAndSpacedThousands()
        {
            const string csv = "name;unused\nA A;12,5\nB B;12.5\nC C;2 865,78\n";

            var rows = VacationEntitlementParser.ParseCsv(csv).Rows;

            Assert.That(rows.Select(row => row.Unused), Is.EqualTo(new[] { 12.5, 12.5, 2865.78 }));
        }

        [Test]
        public void ParseCsv_FallsBackToPositionalColumnsWithoutAHeader()
        {
            const string csv = "E-1;Jonas Petraitis;21\nE-2;Ona Onaitė;3,5\n";

            var rows = VacationEntitlementParser.ParseCsv(csv).Rows;

            Assert.That(rows.Count, Is.EqualTo(2));
            Assert.That(rows[0].Code, Is.EqualTo("E-1"));
            Assert.That(rows[1].Unused, Is.EqualTo(3.5));
        }

        [Test]
        public void ParseCsv_CountsALineItCannotReadRatherThanGuessing()
        {
            const string csv = "name;unused\nJonas Petraitis;not a number\nOna Onaitė;4\n";

            var result = VacationEntitlementParser.ParseCsv(csv);

            Assert.That(result.Rows.Count, Is.EqualTo(1));
            Assert.That(result.Unreadable, Is.EqualTo(1));
        }

        [Test]
        public void ParseCsv_HandlesAnEmptyFile()
        {
            var result = VacationEntitlementParser.ParseCsv(string.Empty);

            Assert.That(result.Rows, Is.Empty);
            Assert.That(result.Unreadable, Is.EqualTo(0));
        }

        [Test]
        public void ParseCsv_UnwrapsAQuotedCellContainingTheSeparator()
        {
            const string csv = "name;unused\n\"Petraitis; Jonas\";7\n";

            var rows = VacationEntitlementParser.ParseCsv(csv).Rows;

            Assert.That(rows.Single().Name, Is.EqualTo("Petraitis; Jonas"));
            Assert.That(rows.Single().Unused, Is.EqualTo(7));
        }

        /// <summary>
        /// Payroll writes the Lithuanian "1,5"; a spreadsheet may re-save it as
        /// "1,234.50". Whichever separator comes last is the decimal one.
        /// </summary>
        [TestCase("5,55", 5.55)]
        [TestCase("5.55", 5.55)]
        [TestCase("1 234,5", 1234.5)]
        [TestCase("1,234.50", 1234.5)]
        [TestCase("1.234,50", 1234.5)]
        [TestCase("7", 7)]
        public void ParseCsv_ReadsEitherDecimalSeparator(string cell, double expected)
        {
            var rows = VacationEntitlementParser.ParseCsv($"name;unused\nJonaitis Jonas;{cell}\n").Rows;

            Assert.That(rows.Single().Unused, Is.EqualTo(expected));
        }

        [Test]
        public void Normalize_StripsDiacriticsAndCase()
        {
            Assert.That(
                VacationEntitlementParser.Normalize("Rūta Trumpauskaitė"),
                Is.EqualTo(VacationEntitlementParser.Normalize("ruta trumpauskaite")));
        }
    }
}
