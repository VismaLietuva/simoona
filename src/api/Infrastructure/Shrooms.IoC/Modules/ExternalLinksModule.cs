using Autofac;
using Shrooms.Domain.Services.ExternalLinks;

namespace Shrooms.IoC.Modules
{
    public class ExternalLinksModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExternalLinkService>().As<IExternalLinkService>().InstancePerRequest();
        }
    }
}
