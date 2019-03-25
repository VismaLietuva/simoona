using RazorEngine;
using RazorEngine.Templating;
using Shrooms.DataTransferObjects.EmailTemplateViewModels;

namespace Shrooms.Infrastructure.Email.Templating
{
    public class MailTemplate : IMailTemplate
    {
        public string Generate<T>(T viewModel, string key)
            where T : BaseEmailTemplateViewModel
        {
            return Engine.Razor.Run(key, typeof(T), viewModel);
        }
    }
}
