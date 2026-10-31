using System;
using NUnit.Framework;
using Shrooms.Infrastructure.ExcelGenerator;

namespace Shrooms.Tests.Infrastructure
{
    [TestFixture]
    public class ExcelWorksheetBuilderTests
    {
        [Test]
        public void AsTableWithTotals_RejectsAnEmptySheet()
        {
            var worksheet = new ExcelBuilder().AddWorksheet("Options");

            var exception = Assert.Throws<InvalidOperationException>(() => worksheet.AsTableWithTotals("Options", "Total", "Count"));

            Assert.That(exception.Message, Does.Contain(nameof(ExcelWorksheetBuilder.AsTableWithTotals)));
        }

        [Test]
        public void AsTableWithTotals_NamesTheMissingSummedColumn()
        {
            var worksheet = new ExcelBuilder()
                .AddWorksheet("Options")
                .AddHeader("Combination", "Count")
                .AddRows(new ExcelRowCollection
                {
                    new ExcelRow
                    {
                        new ExcelColumn { Value = "Pizza" },
                        new ExcelColumn { Value = 2 }
                    }
                });

            var exception = Assert.Throws<ArgumentException>(() => worksheet.AsTableWithTotals("Options", "Total", "Kiekis"));

            Assert.That(exception.Message, Does.Contain("'Kiekis'"));
            Assert.That(exception.ParamName, Is.EqualTo("summedColumns"));
        }
    }
}
