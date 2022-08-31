using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure.Email;

namespace Shrooms.Infrastructure.Email.Templates
{
    public class MailTemplate : IMailTemplate
    {
        private readonly IMailTemplateCache _mailTemplateCache;

        public MailTemplate(IMailTemplateCache mailTemplateCache)
        {
            _mailTemplateCache = mailTemplateCache;
        }

        public string Generate<T>(T viewModel) where T : BaseEmailTemplateViewModel
        {
            return _mailTemplateCache.Get<T>().Run(viewModel);
        }
    }
}
