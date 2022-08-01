namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelColumnInternal
    {
        public int RowIndex { get; set; }

        public int ColumnIndex { get; set; }

        public IExcelColumn Column { get; set; }
    }
}
