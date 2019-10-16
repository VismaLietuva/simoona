using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelBuilder : IDisposable, IExcelBuilder
    {
        private ExcelPackage _package;

        public ExcelBuilder()
        {
            _package = new ExcelPackage();
        }

        public IExcelBuilder AddNewWorksheet(string sheetName, IEnumerable<string> headerItems, IEnumerable<IEnumerable<object>> rows)
        {
            var worksheet = _package.Workbook.Worksheets.Add(sheetName);
            var worksheetBuilder = new ExcelWorksheetBuilder(worksheet);

            if (headerItems.Count() == 0)
            {
                worksheetBuilder
                    .WithRows(rows)
                    .Build();
            }
            else
            {
                worksheetBuilder
                    .WithHeader(headerItems)
                    .WithRows(rows)
                    .Build();
            }

            return this;
        }

        public IExcelBuilder AddNewWorksheet(string sheetName, IEnumerable<IEnumerable<object>> rows)
        {
            var worksheet = _package.Workbook.Worksheets.Add(sheetName);
            var worksheetBuilder = new ExcelWorksheetBuilder(worksheet);

            worksheetBuilder
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
