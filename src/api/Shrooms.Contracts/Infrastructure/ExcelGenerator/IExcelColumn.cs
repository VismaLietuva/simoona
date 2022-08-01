namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelColumn
    {
        public string Format { get; set; }

        public bool SetBoldFont { get; set; }

        public bool SetHorizontalTextCenter { get; set; }

        public bool SetVerticalTextCenter { get; set; }

        public object Value { get; set; }
    }
}
