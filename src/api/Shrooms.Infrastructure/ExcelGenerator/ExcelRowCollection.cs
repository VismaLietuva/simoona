using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using System.Collections;
using System.Collections.Generic;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelRowCollection : IExcelRowCollection
    {
        public IList<IExcelRow> Rows { get; }

        public ExcelRowCollection(List<IExcelRow> rows)
        {
            Rows = rows;
        }

        public ExcelRowCollection()
        {
            Rows = new List<IExcelRow>();
        }

        public IExcelRow this[int index]
        {
            get => Rows[index];
        }

        public int Count
        {
            get => Rows.Count;
        }

        public void Add(IExcelRow excelRow)
        {
            Rows.Add(excelRow);
        }

        public IEnumerator<IExcelColumnInternal> GetEnumerator()
        {
            var currentRowIndex = 0;

            foreach (var row in Rows)
            {
                currentRowIndex++;

                var currentColumnIndex = 1;

                foreach (var cell in row)
                {
                    yield return new ExcelColumnInternal
                    {
                        RowIndex = currentRowIndex,
                        ColumnIndex = currentColumnIndex++,
                        Column = cell
                    };
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
