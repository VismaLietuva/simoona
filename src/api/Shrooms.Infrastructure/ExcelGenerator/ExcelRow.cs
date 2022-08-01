using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using System.Collections;
using System.Collections.Generic;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelRow : IExcelRow
    {
        private readonly IList<IExcelColumn> _columns;

        public ExcelRow()
        {
            _columns = new List<IExcelColumn>();
        }

        public void Add(IExcelColumn column)
        {
            _columns.Add(column);
        }

        public IEnumerator GetEnumerator()
        {
            return _columns.GetEnumerator();
        }

        IEnumerator<IExcelColumn> IEnumerable<IExcelColumn>.GetEnumerator()
        {
            return _columns.GetEnumerator();
        }
    }
}
