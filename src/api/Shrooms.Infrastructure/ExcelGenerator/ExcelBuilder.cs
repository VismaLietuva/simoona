using System.Collections.Generic;
using OfficeOpenXml;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Infrastructure.ExcelGenerator.Styles;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelBuilder : IExcelBuilder
    {
        private readonly ExcelPackage _package;

        public ExcelBuilder()
        {
            _package = new ExcelPackage();
        }

        public IExcelBuilder AddNewWorksheet(string sheetName, IEnumerable<string> headerItems, IEnumerable<IEnumerable<object>> rows)
        {
            var worksheet = _package.Workbook.Worksheets.Add(sheetName);
            var worksheetBuilder = new ExcelWorksheetBuilder(worksheet);
            
            worksheetBuilder
                .WithHeader(headerItems)
                .WithRows(rows)
                .Build();

            return this;
        }

        public IExcelBuilder AddNewWorksheet(string sheetName, IEnumerable<IEnumerable<object>> rows, ExcelBuilderStyles builderStyle)
        {
            var worksheet = _package.Workbook.Worksheets.Add(sheetName);

            new ExcelWorksheetBuilder(worksheet)
                .WithRows(rows)
                .Build();

            ApplyBuilderStyle(worksheet, builderStyle, rows);

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

        private void ApplyBuilderStyle(ExcelWorksheet worksheet, ExcelBuilderStyles builderStyle, IEnumerable<IEnumerable<object>> rows)
        {
            if (TryFindBuilderStyle(builderStyle, rows, out var style))
            {
                style.Apply(worksheet);
            }
        }

        private bool TryFindBuilderStyle(ExcelBuilderStyles builderStyle, IEnumerable<IEnumerable<object>> rows, out ExcelBuilderStyleBase style)
        {
            switch (builderStyle)
            {
                case ExcelBuilderStyles.LotteryParticipants:
                    style = new LotteryParticipantsBuilderStyle(rows);
                    break;
                case ExcelBuilderStyles.Default:
                default:
                    style = null;
                    return false;
            }

            return true;
        }
    }
}
