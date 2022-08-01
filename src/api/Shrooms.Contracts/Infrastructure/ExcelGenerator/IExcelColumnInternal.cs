namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelColumnInternal
    {
        int RowIndex { get; set; }

        int ColumnIndex { get; set; }

        IExcelColumn Column { get; set; }
    }
}
