using System.Collections.Generic;

namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelRow
    {
        IList<IExcelColumn> Columns { get; set; }
    }
}
