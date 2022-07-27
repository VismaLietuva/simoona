using Shrooms.Contracts.Enums;
using System;
using System.Collections.Generic;

namespace Shrooms.Contracts.Infrastructure
{
    public interface IExcelBuilder : IDisposable
    {
        IExcelBuilder AddNewWorksheet(string sheetName, IEnumerable<string> headerItems, IEnumerable<IEnumerable<object>> rows);

        IExcelBuilder AddNewWorksheet(string sheetName, IEnumerable<IEnumerable<object>> rows, ExcelBuilderStyles builderStyle);

        byte[] GenerateByteArray();
    }
}