using Autofac;
using Shrooms.Domain.Services.Support;

namespace Shrooms.IoC.Modules
{
    public class SupportModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SupportService>().As<ISupportService>().InstancePerRequest();
        }
    }
}