using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using System.Collections.Generic;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelRow : IExcelRow
    {
        public ExcelRow()
        {
            Columns = new List<IExcelColumn>();
        }

        public IList<IExcelColumn> Columns { get; set; }
    }
}
