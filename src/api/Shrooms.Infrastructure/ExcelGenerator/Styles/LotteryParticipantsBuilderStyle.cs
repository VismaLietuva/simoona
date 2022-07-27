using OfficeOpenXml;
using System;
using System.Linq;
using System.Collections.Generic;
using OfficeOpenXml.Style;
using Shrooms.Infrastructure.Extensions;

namespace Shrooms.Infrastructure.ExcelGenerator.Styles
{
    public class LotteryParticipantsBuilderStyle : ExcelBuilderStyleBase
    {
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

            using var excelRange = worksheet.Cells;

            foreach (var position in _rows.GetRowsPositionEnumerator())
            {
                ApplyCellStyle(excelRange[position.Item1 + 1, position.Item2 + 1]);
            }

        }

        private void ApplyCellStyle(ExcelRange cellRange)
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
