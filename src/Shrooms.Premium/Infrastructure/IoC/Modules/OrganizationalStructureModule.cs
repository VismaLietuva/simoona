using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.OrganizationalStructure;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
{
    public class OrganizationalStructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OrganizationalStructureService>().As<IOrganizationalStructureService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
