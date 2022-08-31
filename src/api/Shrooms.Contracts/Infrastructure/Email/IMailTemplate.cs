using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Contracts.Infrastructure.Email
{
    public interface IMailTemplate
    {
        string Generate<T>(T viewModel) where T : BaseEmailTemplateViewModel;
    }
}