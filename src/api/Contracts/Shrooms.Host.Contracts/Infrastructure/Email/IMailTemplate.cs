using Shrooms.Host.Contracts.DataTransferObjects;

namespace Shrooms.Host.Contracts.Infrastructure.Email
{
    public interface IMailTemplate
    {
        string Generate<T>(T viewModel, string key)
            where T : BaseEmailTemplateViewModel;
    }
}