using Shrooms.Contracts.Infrastructure.ExcelGenerator;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelColumnInternal : IExcelColumnInternal
    {
        public int RowIndex { get; set; }
        
        public int ColumnIndex { get; set; }

        public IExcelColumn Column { get; set; }
    }
}
