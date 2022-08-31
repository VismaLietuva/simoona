using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Contracts.Infrastructure.Email
{
    public interface ICompiledEmailTemplate
    {
        string Run<T>(T model) where T : BaseEmailTemplateViewModel;
    } 
}
