namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelColumn
    {
        string Format { get; set; }

        bool SetBoldFont { get; set; }

        bool SetHorizontalTextCenter { get; set; }

        bool SetVerticalTextCenter { get; set; }

        object Value { get; set; }
    }
}
