using Shrooms.DataTransferObjects.EmailTemplateViewModels;

namespace Shrooms.Infrastructure.Email.Templating
{
    public interface IMailTemplate
    {
        string Generate<T>(T viewModel, string key)
            where T : BaseEmailTemplateViewModel;
    }
}