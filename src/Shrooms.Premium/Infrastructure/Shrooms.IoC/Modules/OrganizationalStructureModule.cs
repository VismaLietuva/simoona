using Autofac;
using Shrooms.Domain.Services.OrganizationalStructure;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class OrganizationalStructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OrganizationalStructureService>().As<IOrganizationalStructureService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
