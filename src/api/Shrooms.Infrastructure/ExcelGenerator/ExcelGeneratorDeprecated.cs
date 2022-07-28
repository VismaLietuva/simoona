using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelGeneratorDeprecated : IDisposable
    {
        private const string FirstCell = "A1";
        private const string DefaulDateFormat = "yyyy-mm-dd";
        private const string DefaultNumberFormat = "#";

        private readonly ExcelPackage _package;
        private readonly ExcelWorksheet _worksheet;
        private readonly bool _autofit;

        private int _rowNum;
        private string _lastLetter;
        private List<int> _columnsToCenter;

        public ExcelGeneratorDeprecated(string sheetName, bool autofit = true)
        {
            _package = new ExcelPackage();
            _worksheet = _package.Workbook.Worksheets.Add(sheetName);
            _autofit = autofit;
            _rowNum = 0;
            _columnsToCenter = new List<int>();
        }

        public void AddRow(IList<object> rowItems)
        {
            _rowNum++;
            var letter = string.Empty;

            for (var i = 0; i < rowItems.Count; i++)
            {
                letter = IndexToLetter(i);
                AddCell(letter, _rowNum, rowItems[i]);
            }

            if (string.IsNullOrEmpty(_lastLetter))
            {
                _lastLetter = letter;
            }
        }

        public void AddHeaderRow(IList<string> headerItems)
        {
            _rowNum = 0;
            AddRow(headerItems.Select(i => (object)i).ToList());
            SetHeaderRowStyle($"{FirstCell}:{_lastLetter}{_rowNum}");
        }

        public byte[] Generate()
        {
            AutofitAll();
            CenterColumns();

            return _package.GetAsByteArray();
        }

        private void CenterColumns()
        {
            foreach (var columnIndex in _columnsToCenter)
            {
                var letter = IndexToLetter(columnIndex);

                _worksheet.Cells[$"{letter}2:{letter}{_rowNum}"]
                    .Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
        }

        public void AddCell(string letter, int number, object value)
        {
            var cellLocation = letter + number;

            if (value == null)
            {
                return;
            }

            if (value is DateTime)
            {
                _worksheet.Cells[cellLocation].Value = value;
                _worksheet.Cells[cellLocation].Style.Numberformat.Format = DefaulDateFormat;
            }
            else if (value is int || value is long)
            {
                _worksheet.Cells[cellLocation].Value = value;
                _worksheet.Cells[cellLocation].Style.Numberformat.Format = DefaultNumberFormat;
            }
            else
            {
                _worksheet.Cells[cellLocation].Value = value.ToString();
            }
        }

        public void CenterColumns(params int[] indexes)
        {
            if (indexes != null)
            {
                _columnsToCenter = indexes.ToList();
            }
        }

        private void AutofitAll()
        {
            if (!_autofit)
            {
                return;
            }

            _worksheet
                .Cells[$"{FirstCell}:{_lastLetter}{_rowNum}"]
                .AutoFitColumns();
        }

        private static string IndexToLetter(int index)
        {
            if (index < 0 || index > 26)
            {
                throw new IndexOutOfRangeException("Must be in range [0 - 26]");
            }

            var letter = (char)('A' + (char)(index % 26));

            return letter.ToString();
        }

        private void SetHeaderRowStyle(string range)
        {
            _worksheet.Cells[range].Style.Font.Bold = true;
            _worksheet.Cells[range].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        public void Dispose()
        {
            _package?.Dispose();
        }
    }
}