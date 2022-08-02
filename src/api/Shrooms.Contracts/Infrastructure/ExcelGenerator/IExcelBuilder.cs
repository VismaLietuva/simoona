namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelBuilder
    {
        IExcelWorksheetBuilder AddWorksheet(string sheetName);

        byte[] Build();
    }
}
