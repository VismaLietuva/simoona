using OfficeOpenXml;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using Shrooms.Infrastructure.ExcelGenerator.Styles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelWorksheetBuilder : IExcelWorksheetBuilder
    {
        private const string DefaulDateFormat = "yyyy-mm-dd HH:mm:ss";
        private const string DefaultNumberFormat = "#";
        private const string DefaultDecimalFormat = "0.00";

        private const int MaximumTextCharacterLength = 70;

        private const int MinimmumColumnWidth = 5;
        private const int MaximumColumnWidth = 60;

        private readonly ExcelWorksheet _worksheet;
        private readonly IEnumerable<IEnumerable<object>> _rows;

        private readonly bool _isHeaderPresent;

        public ExcelWorksheetBuilder(
            ExcelWorksheet worksheet,
            bool isHeaderPresent = false, 
            IEnumerable<IEnumerable<object>> rows = null)
        {
            _worksheet = worksheet;
            _isHeaderPresent = isHeaderPresent;
            _rows = rows;
        }

        public IExcelWorksheetBuilder AddHeader(params string[] headerParts)
        {
            return AddHeader(headerParts.AsEnumerable());
        }

        public IExcelWorksheetBuilder AddHeader(IEnumerable<string> headerParts)
        {
            var index = 1;

            foreach (var item in headerParts)
            {
                _worksheet.Cells[1, index].Value = item;
                index++;
            }

            _worksheet.Cells[1, 1, 1, index - 1].AutoFilter = true;
            _worksheet.Cells[1, 1, 1, index - 1].Style.Font.Bold = true;

            return new ExcelWorksheetBuilder(_worksheet, true, _rows);
        }

        public IExcelWorksheetBuilder AddRows(IEnumerable<IEnumerable<object>> rows)
        {
            foreach (var cell in GetCellEnumerator(rows))
            {
                AddCellContent(cell);
            }

            return new ExcelWorksheetBuilder(_worksheet, _isHeaderPresent, rows);
        }

        public IExcelWorksheetBuilder AddStyle(ExcelBuilderStyles excelBuilderStyle = ExcelBuilderStyles.Default)
        {
            if (TryFindBuilderStyle(excelBuilderStyle, _rows, out var style))
            {
                style.Apply(_worksheet);
            }

            return this;
        }

        public IExcelWorksheetBuilder AutoFitColumns(int minimumWidth, int maximumWidth)
        {
            _worksheet.Cells.AutoFitColumns(minimumWidth, maximumWidth);

            return this;
        }

        public IExcelWorksheetBuilder AutoFitColumns()
        {
            return AutoFitColumns(MinimmumColumnWidth, MaximumColumnWidth);
        }

        private void AddCellContent(ExcelCell cell)
        {
            var cellRange = _worksheet.Cells[cell.RowIndex, cell.ColumnIndex];

            if (cell.Value == null)
            {
                cell.Value = string.Empty;
                
                return;
            }

            if (TryFindNumberFormat(cell.Value.GetType(), out var format))
            {
                cellRange.Value = cell.Value;
                cellRange.Style.Numberformat.Format = format;

                return;
            }

            var text = cell.Value.ToString();

            cellRange.Value = text;

            if (text.Length > MaximumTextCharacterLength)
            {
                cellRange.Style.WrapText = true;
            }
        }

        private bool TryFindNumberFormat(Type type, out string format)
        {
            format = null;

            if (type == typeof(DateTime))
            {
                format = DefaulDateFormat;
            }
            else if (type == typeof(long) || type == typeof(int))
            {
                format = DefaultNumberFormat;
            }
            else if (type == typeof(decimal))
            {
                format = DefaultDecimalFormat;
            }

            return format != null;
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

        private IEnumerable<ExcelCell> GetCellEnumerator<T>(IEnumerable<IEnumerable<T>> enumerable)
        {
            var currentRowIndex = _isHeaderPresent ? 1 : 0;

            foreach (var row in enumerable)
            {
                currentRowIndex++;

                var currentColumnIndex = 1;

                foreach (var column in row)
                {
                    yield return new ExcelCell
                    {
                        RowIndex = currentRowIndex,
                        ColumnIndex = currentColumnIndex++,
                        Value = column
                    };
                }
            }
        }
    }
}
