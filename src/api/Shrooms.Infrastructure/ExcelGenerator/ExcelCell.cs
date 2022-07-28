namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelCell
    {
        public int RowIndex { get; set; }

        public int ColumnIndex { get; set; }
        
        public object Value { get; set; }
    }
}
