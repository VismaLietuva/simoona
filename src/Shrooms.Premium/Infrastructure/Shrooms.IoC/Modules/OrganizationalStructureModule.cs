using Autofac;
using Shrooms.Domain.Services.OrganizationalStructure;

namespace Shrooms.IoC.Modules
{
    public class OrganizationalStructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OrganizationalStructureService>().As<IOrganizationalStructureService>().InstancePerRequest();
        }
    }
}
