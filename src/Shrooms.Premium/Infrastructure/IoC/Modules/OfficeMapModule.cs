using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.OfficeMap;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
{
    public class OfficeMapModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OfficeMapService>().As<IOfficeMapService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
