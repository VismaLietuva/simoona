using RazorEngineCore;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure.Email;

namespace Shrooms.Infrastructure.Email.Templates
{
    public class CompiledEmailTemplate<TDestination> : ICompiledEmailTemplate
        where TDestination : BaseEmailTemplateViewModel
    {
        private readonly IRazorEngineCompiledTemplate<EmailTemplateBase<TDestination>> _compiledTemplate;
        private readonly IRazorEngineCompiledTemplate<EmailTemplateBase<LayoutEmailTemplateViewModel>> _compiledLayout;

        public CompiledEmailTemplate(
            IRazorEngineCompiledTemplate<EmailTemplateBase<TDestination>> compiledTemplate,
            IRazorEngineCompiledTemplate<EmailTemplateBase<LayoutEmailTemplateViewModel>> compiledLayout = null)
        {
            _compiledTemplate = compiledTemplate;
            _compiledLayout = compiledLayout;
        }

        public string Run<T>(T model) where T : BaseEmailTemplateViewModel
        {
            return Run(model as TDestination);
        }

        private string Run(TDestination model)
        {
            var compiled = _compiledTemplate.Run(instance =>
            {
                instance.Model = model;
            });

            if (_compiledLayout == null)
            {
                return compiled;
            }

            return _compiledLayout.Run(instance =>
            {
                instance.Model = new LayoutEmailTemplateViewModel(model.UserNotificationSettingsUrl);
                instance.RenderBodyCallback = () => compiled;
            });
        }
    }
}
