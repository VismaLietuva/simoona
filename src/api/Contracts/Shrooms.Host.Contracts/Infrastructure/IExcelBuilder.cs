using System;
using System.Collections.Generic;

namespace Shrooms.Host.Contracts.Infrastructure
{
    public interface IExcelBuilder : IDisposable
    {
        IExcelBuilder AddNewWorksheet(string sheetName, IEnumerable<string> headerItems, IEnumerable<IEnumerable<object>> rows);

        byte[] GenerateByteArray();
    }
}
