using System;
using System.Collections.Generic;
using OfficeOpenXml;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelWorksheetBuilder
    {
        private readonly ExcelWorksheet _worksheet;
        private bool _withHeader;

        private const string DefaulDateFormat = "yyyy-mm-dd HH:mm:ss";
        private const string DefaultNumberFormat = "#";
        private const string DefaultDecimalFormat = "0.00";

        public ExcelWorksheetBuilder(ExcelWorksheet worksheet)
        {
            _worksheet = worksheet;
            _withHeader = false;
        }

        public ExcelWorksheetBuilder WithHeader(IEnumerable<string> headerItems)
        {
            _withHeader = true;
            var index = 1;
            foreach (var item in headerItems)
            {
                _worksheet.Cells[1, index].Value = item;
                index++;
            }

            _worksheet.Cells[1, 1, 1, index - 1].AutoFilter = true;
            _worksheet.Cells[1, 1, 1, index - 1].Style.Font.Bold = true;

            return this;
        }

        public ExcelWorksheetBuilder WithRows(IEnumerable<IEnumerable<object>> rows)
        {
            var rowIndex = _withHeader ? 2 : 1;

            foreach (var row in rows)
            {
                WithRow(rowIndex++, row);
            }

            return this;
        }

        public ExcelWorksheetBuilder WithRow(int rowIndex, IEnumerable<object> row)
        {
            var columnIndex = 1;
            foreach (var columnValue in row)
            {
                if (columnValue == null)
                {
                    _worksheet.Cells[rowIndex, columnIndex].Value = string.Empty;
                }
                else if (columnValue is DateTime)
                {
                    _worksheet.Cells[rowIndex, columnIndex].Value = columnValue;
                    _worksheet.Cells[rowIndex, columnIndex].Style.Numberformat.Format = DefaulDateFormat;
                }
                else if (columnValue is int || columnValue is long)
                {
                    _worksheet.Cells[rowIndex, columnIndex].Value = columnValue;
                    _worksheet.Cells[rowIndex, columnIndex].Style.Numberformat.Format = DefaultNumberFormat;
                }
                else if (columnValue is decimal)
                {
                    _worksheet.Cells[rowIndex, columnIndex].Value = columnValue;
                    _worksheet.Cells[rowIndex, columnIndex].Style.Numberformat.Format = DefaultDecimalFormat;
                }
                else
                {
                    var text = columnValue.ToString();
                    _worksheet.Cells[rowIndex, columnIndex].Value = text;

                    // Wrap text if it is too long.
                    if (text.Length > 70)
                    {
                        _worksheet.Cells[rowIndex, columnIndex].Style.WrapText = true;
                    }
                }

                columnIndex++;
            }

            return this;
        }

        public ExcelWorksheet Build()
        {
            _worksheet.Cells.AutoFitColumns(5, 60);
            return _worksheet;
        }
    }
}
