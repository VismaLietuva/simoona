using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelColumn : IExcelColumn
    {
        public string Format { get; set; }

        public bool SetBoldFont { get; set; }

        public bool SetHorizontalTextCenter { get; set; }

        public bool SetVerticalTextCenter { get; set; }

        public bool WrapText { get; set; }

        public object Value { get; set; }

        public ExcelBorderStylePicker BorderTop { get; set; }

        public ExcelBorderStylePicker BorderBottom { get; set; }

        public ExcelBorderStylePicker BorderLeft { get; set; }

        public ExcelBorderStylePicker BorderRight { get; set; }
    }
}
