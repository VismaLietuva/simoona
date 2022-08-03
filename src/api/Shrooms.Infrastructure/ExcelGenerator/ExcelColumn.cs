using Shrooms.Contracts.Infrastructure.ExcelGenerator;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelColumn : IExcelColumn
    {
        public string Format { get; set; }

        public bool SetBoldFont { get; set; }

        public bool SetHorizontalTextCenter { get; set; }

        public bool SetVerticalTextCenter { get; set; }

        public object Value { get; set; }
    }
}
