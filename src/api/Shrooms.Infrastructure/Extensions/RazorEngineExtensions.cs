using RazorEngineCore;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Templates;
using System;

namespace Shrooms.Infrastructure.Extensions
{
    public static class RazorEngineExtensions
    {
        public static ICompiledEmailTemplate Compile<T>(
            this IRazorEngine razorEngine,
            string template, 
            IRazorEngineCompiledTemplate<EmailTemplateBase<LayoutEmailTemplateViewModel>> layoutTemplate,
            Action<IRazorEngineCompilationOptionsBuilder> builder = null)
            where T : BaseEmailTemplateViewModel
        {
            return new CompiledEmailTemplate<T>(
                razorEngine.Compile<EmailTemplateBase<T>>(template, builder),
                layoutTemplate);
        }
    }
}
