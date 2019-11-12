using System.Collections.Generic;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public interface IExcelBuilder
    {
        IExcelBuilder AddNewWorksheet(string sheetName, IEnumerable<string> headerItems, IEnumerable<IEnumerable<object>> rows);
        IExcelBuilder AddNewWorksheet(string sheetName, IEnumerable<IEnumerable<object>> rows);


        byte[] GenerateByteArray();
    }
}
