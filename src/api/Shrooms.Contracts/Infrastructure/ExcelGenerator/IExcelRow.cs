using System.Collections.Generic;

namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelRow : IEnumerable<IExcelColumn>
    {
        void Add(IExcelColumn column);
    }
}
