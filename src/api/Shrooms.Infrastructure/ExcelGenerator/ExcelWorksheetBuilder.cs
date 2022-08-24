using OfficeOpenXml;
using OfficeOpenXml.Style;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelWorksheetBuilder : IExcelWorksheetBuilder
    {
        private readonly ExcelWorksheet _worksheet;
        private readonly IExcelRowCollection _excelRowCollection;

        private readonly bool _isHeaderPresent;

        public ExcelWorksheetBuilder(
            ExcelWorksheet worksheet,
            bool isHeaderPresent = false, 
            IExcelRowCollection excelRowCollection = null)
        {
            _worksheet = worksheet;
            _isHeaderPresent = isHeaderPresent;
            _excelRowCollection = excelRowCollection;
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

            return new ExcelWorksheetBuilder(_worksheet, true, _excelRowCollection);
        }

        public IExcelWorksheetBuilder AutoFitColumns(int minimumWidth, int maximumWidth)
        {
            _worksheet.Cells.AutoFitColumns(minimumWidth, maximumWidth);

            return this;
        }

        public IExcelWorksheetBuilder AutoFitColumns()
        {
            return AutoFitColumns(ExcelWorksheetBuilderConstants.MinimmumColumnWidth, ExcelWorksheetBuilderConstants.MaximumColumnWidth);
        }
        
        public IExcelWorksheetBuilder AddRows<T>(IQueryable<T> data, Expression<Func<T, IExcelRow>> expression, bool applyAutoFit = true)
        {
            return AddRows(new ExcelRowCollection(data.Select(expression).ToList()), applyAutoFit);
        }

        public IExcelWorksheetBuilder AddRows(IExcelRowCollection rowCollection, bool applyAutoFit = true)
        {
            foreach (var internalCell in rowCollection)
            {
                AddColumnContent(internalCell);
            }

            if (applyAutoFit)
            {
                AutoFitColumns();
            }

            return new ExcelWorksheetBuilder(_worksheet, _isHeaderPresent, rowCollection);
        }

        public IExcelWorksheetBuilder AddColumnsPadding(params double[] paddings)
        {
            var columnIndex = 1;

            foreach (var padding in paddings)
            {
                _worksheet.Column(columnIndex++).Width += padding;
            }

            return this;
        }

        public IExcelWorksheetBuilder AddColumnSequence<T>(IEnumerable<T> data, Func<T, IExcelColumn> mapFuction, int moveToNextRowAfter, bool applyAutoFit = true)
        {
            var excelRowCollection = new ExcelRowCollection();
            var excelRow = new ExcelRow();
            var columnIndex = 0;

            foreach (var column in data)
            {
                if (columnIndex % moveToNextRowAfter == 0 && columnIndex != 0)
                {
                    excelRowCollection.Add(excelRow);
                    excelRow = new ExcelRow();
                }

                excelRow.Add(mapFuction(column));
                columnIndex++;
            }

            if (excelRow.Any())
            {
                excelRowCollection.Add(excelRow);
            }

            return AddRows(excelRowCollection, applyAutoFit);
        }

        public IExcelWorksheetBuilder AddRowPadding(double padding)
        {
            if (_excelRowCollection == null)
            {
                throw new InvalidOperationException($"{nameof(AddRows)} or {nameof(AddColumnSequence)} has to be called");
            }

            var rowIndex = 1;

            foreach (var excelRow in _excelRowCollection.Rows)
            {
                _worksheet.Row(rowIndex++).Height += padding;
            }

            return this;
        }

        public IExcelWorksheetBuilder AddColumnsPadding(double padding)
        {
            if (_excelRowCollection == null)
            {
                throw new InvalidOperationException($"{nameof(AddRows)} or {nameof(AddColumnSequence)} has to be called");
            }

            if (!_excelRowCollection.Rows.Any())
            {
                return this;
            }

            var maxColumnCount = _excelRowCollection.Rows
                .Max(row => row.Count());

            return AddColumnPadding(padding, maxColumnCount);
        }

        public IExcelWorksheetBuilder AddColumnPadding(double padding, int columnCount)
        {
            for (var i = 1; i <= columnCount; i++)
            {
                _worksheet.Column(i).Width += padding;
            }

            return this;
        }

        private void AddColumnContent(IExcelColumnInternal internalColumn)
        {
            var columnRange = GetColumnRange(internalColumn);
            var columnStyle = columnRange.Style;

            columnRange.Value = internalColumn.Column.Value ?? string.Empty;
            columnRange.Style.Numberformat.Format = internalColumn.Column.Format ?? columnRange.Style.Numberformat.Format;
            columnRange.Style.Font.Bold = internalColumn.Column.SetBoldFont;
            columnRange.Style.VerticalAlignment = internalColumn.Column.SetHorizontalTextCenter ? ExcelVerticalAlignment.Center : columnRange.Style.VerticalAlignment;
            columnRange.Style.HorizontalAlignment = internalColumn.Column.SetHorizontalTextCenter ? ExcelHorizontalAlignment.Center : columnRange.Style.HorizontalAlignment;

            columnStyle.WrapText = internalColumn.Column.WrapText;

            if (internalColumn.Column.Value is string stringValue && stringValue.Length > ExcelWorksheetBuilderConstants.MaximumTextCharacterLength)
            {
                columnStyle.WrapText = true;
            }

            ApplyBorderStyles(columnRange, internalColumn);
        }

        private void ApplyBorderStyles(ExcelRange columnRange, IExcelColumnInternal internalColumn)
        {
            columnRange.Style.Border.Top.Style = GetBorderStyle(internalColumn.Column.BorderTop);
            columnRange.Style.Border.Bottom.Style = GetBorderStyle(internalColumn.Column.BorderBottom);
            columnRange.Style.Border.Left.Style = GetBorderStyle(internalColumn.Column.BorderLeft);
            columnRange.Style.Border.Right.Style = GetBorderStyle(internalColumn.Column.BorderRight);
        }

        private ExcelBorderStyle GetBorderStyle(ExcelBorderStylePicker excelBorder)
        {
            return excelBorder switch
            {
                ExcelBorderStylePicker.None => ExcelBorderStyle.None,
                ExcelBorderStylePicker.Thin => ExcelBorderStyle.Thin,
                _ => ExcelBorderStyle.None,
            };
        }

        private ExcelRange GetColumnRange(IExcelColumnInternal internalColumn)
        {
            if (!_isHeaderPresent)
            {
                return _worksheet.Cells[internalColumn.RowIndex, internalColumn.ColumnIndex];
            }

            return _worksheet.Cells[internalColumn.RowIndex + 1, internalColumn.ColumnIndex];
        }
    }
}
