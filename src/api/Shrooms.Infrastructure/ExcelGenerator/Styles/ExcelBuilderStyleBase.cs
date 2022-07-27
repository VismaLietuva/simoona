using OfficeOpenXml;

namespace Shrooms.Infrastructure.ExcelGenerator.Styles
{
    public abstract class ExcelBuilderStyleBase
    {
        public abstract void Apply(ExcelWorksheet worksheet);
    }
}
