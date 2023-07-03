namespace Shrooms.Contracts.Infrastructure.ExcelGenerator
{
    public interface IExcelBuilderFactory
    {
        IExcelBuilder GetBuilder();
    }
}
