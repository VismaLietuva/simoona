using OfficeOpenXml;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelBuilder : IExcelBuilder
    {
        private readonly ExcelPackage _package;

        public ExcelBuilder()
        {
            _package = new ExcelPackage();
        }

        public byte[] Build()
        {
            return _package.GetAsByteArray();
        }

        public IExcelWorksheetBuilder AddWorksheet(string sheetName)
        {
            var worksheet = _package.Workbook.Worksheets.Add(sheetName);

            return new ExcelWorksheetBuilder(worksheet);
        }
    }
}
