using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelColumn
    {
        string Format { get; set; }

        bool SetBoldFont { get; set; }

        bool SetHorizontalTextCenter { get; set; }

        bool SetVerticalTextCenter { get; set; }

        object Value { get; set; }

        ExcelBorderStylePicker BorderTop { get; set; }

        ExcelBorderStylePicker BorderBottom { get; set; }

        ExcelBorderStylePicker BorderLeft { get; set; }

        ExcelBorderStylePicker BorderRight { get; set; }
    }
}
