using Autofac;
using Shrooms.Domain.Services.Badges;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
{
    public class BadgesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BadgesService>().As<IBadgesService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
