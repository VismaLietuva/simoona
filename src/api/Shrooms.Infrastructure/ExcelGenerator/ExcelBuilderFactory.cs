using OfficeOpenXml;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;

namespace Shrooms.Infrastructure.ExcelGenerator
{
    public class ExcelBuilderFactory : IExcelBuilderFactory
    {
        public IExcelBuilder GetBuilder()
        {
            return new ExcelBuilder();
        }
    }
}
