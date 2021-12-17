using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Domain.Services.KudosShop;

namespace Shrooms.Premium.IoC.Modules
{
    public class KudosShopModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KudosShopService>().As<IKudosShopService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}