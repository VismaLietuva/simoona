using Autofac;
using Shrooms.Infrastructure.Interceptors;
using Shrooms.Premium.Domain.Services.OrganizationalStructure;

namespace Shrooms.Premium.IoC.Modules
{
    public class OrganizationalStructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OrganizationalStructureService>().As<IOrganizationalStructureService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
