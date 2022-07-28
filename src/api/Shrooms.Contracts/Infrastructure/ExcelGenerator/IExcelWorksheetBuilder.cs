using Shrooms.Contracts.Enums;
using System.Collections.Generic;

namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelWorksheetBuilder
    {
        IExcelWorksheetBuilder AddHeader(params string[] headerParts);

        IExcelWorksheetBuilder AddHeader(IEnumerable<string> headerParts);

        IExcelWorksheetBuilder AddRows(IEnumerable<IEnumerable<object>> rows);

        IExcelWorksheetBuilder AutoFitColumns(int minimumWidth, int maximumWidth);

        IExcelWorksheetBuilder AutoFitColumns();

        IExcelWorksheetBuilder AddStyle(ExcelBuilderStyles excelBuilderStyle = ExcelBuilderStyles.Default);
    }
}
