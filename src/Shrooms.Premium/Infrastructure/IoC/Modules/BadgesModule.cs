using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.Badges;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
{
    public class BadgesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BadgesService>().As<IBadgesService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
