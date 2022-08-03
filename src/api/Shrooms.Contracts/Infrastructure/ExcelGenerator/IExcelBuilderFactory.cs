using Shrooms.Contracts.Infrastructure.ExcelGenerator;

namespace Shrooms.Contracts.Infrastructure
{
    public interface IExcelBuilderFactory
    {
        IExcelBuilder GetBuilder();
    }
}