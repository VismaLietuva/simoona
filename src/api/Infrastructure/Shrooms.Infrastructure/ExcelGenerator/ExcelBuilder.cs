using System;
using System.Collections.Generic;
using OfficeOpenXml;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelBuilder : IDisposable, IExcelBuilder
    {
        private readonly ExcelPackage _package;

        public ExcelBuilder()
        {
            _package = new ExcelPackage();
        }

        public IExcelBuilder AddNewWorksheet(
            string sheetName,
            IEnumerable<string> headerItems,
            IEnumerable<IEnumerable<object>> rows)
        {
            var worksheet = _package.Workbook.Worksheets.Add(sheetName);
            var worksheetBuilder = new ExcelWorksheetBuilder(worksheet);

            worksheetBuilder
                .WithHeader(headerItems)
                .WithRows(rows)
                .Build();

            return this;
        }

        public byte[] GenerateByteArray()
        {
            return _package.GetAsByteArray();
        }

        public void Dispose()
        {
            _package?.Dispose();
        }
    }
}
