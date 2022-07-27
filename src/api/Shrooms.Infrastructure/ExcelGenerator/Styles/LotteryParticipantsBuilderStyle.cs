using OfficeOpenXml;
using System.Collections.Generic;
using OfficeOpenXml.Style;
using Shrooms.Infrastructure.Extensions;
using System.Linq;

namespace Shrooms.Infrastructure.ExcelGenerator.Styles
{
    public class LotteryParticipantsBuilderStyle : ExcelBuilderStyleBase
    {
        private const int HeightPadding = 30;
        private const int WidthPadding = 10;
        
        private readonly IEnumerable<IEnumerable<object>> _rows;

        public LotteryParticipantsBuilderStyle(IEnumerable<IEnumerable<object>> rows)
        {
            _rows = rows;
        }

        public override void Apply(ExcelWorksheet worksheet)
        {
            if (_rows == null)
            {
                return;
            }

            ApplyStyles(worksheet);
            IncreaseDefaultCellsSize(worksheet);
        }

        private void IncreaseDefaultCellsSize(ExcelWorksheet worksheet)
        {
            var rowCount = _rows.Count();
            var firstRowColumnCount = _rows.FirstOrDefault()?.Count() ?? 0;

            for (var i = 1; i <= rowCount; i++)
            {
                worksheet.Row(i).Height += HeightPadding;
            }

            for (var i = 1; i <= firstRowColumnCount; i++)
            {
                worksheet.Column(i).Width += WidthPadding;
            }
        }

        private void ApplyStyles(ExcelWorksheet worksheet)
        {
            using var excelRange = worksheet.Cells;

            foreach (var position in _rows.GetRowsPositionEnumerator())
            {
                var realCellPosition = (position.Item1 + 1, position.Item2 + 1);

                ApplyCellStyles(excelRange[realCellPosition.Item1, realCellPosition.Item2]);
            }
        }

        private void ApplyCellStyles(ExcelRange cellRange)
        {
            cellRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            cellRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            cellRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            cellRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            cellRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            cellRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }
    }
}
