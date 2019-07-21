using Autofac;
using Shrooms.Domain.Services.KudosShop;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class KudosShopModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KudosShopService>().As<IKudosShopService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}