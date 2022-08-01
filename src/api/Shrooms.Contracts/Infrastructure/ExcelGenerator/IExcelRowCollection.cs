using System.Collections.Generic;

namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelRowCollection : IEnumerable<IExcelColumnInternal>
    {
        IExcelRow this[int index] { get; }

        IList<IExcelRow> Rows { get; }

        int Count { get; }

        void Add(IExcelRow excelRow);
    }
}
