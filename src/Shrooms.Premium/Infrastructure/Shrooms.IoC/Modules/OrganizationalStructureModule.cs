using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.OrganizationalStructure;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
{
    public class OrganizationalStructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OrganizationalStructureService>().As<IOrganizationalStructureService>().InstancePerRequest();
        }
    }
}
