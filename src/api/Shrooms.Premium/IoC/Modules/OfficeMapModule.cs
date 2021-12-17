using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Domain.Services.OfficeMap;

namespace Shrooms.Premium.IoC.Modules
{
    public class OfficeMapModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OfficeMapService>().As<IOfficeMapService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
