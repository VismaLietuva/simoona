using System.Collections.Generic;

namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelRowCollection : IEnumerable<IExcelColumnInternal>
    {
        IExcelRow this[int index] { get; }

        public IList<IExcelRow> Rows { get; }

        public int Count { get; }

        void Add(IExcelRow excelRow);
    }
}
