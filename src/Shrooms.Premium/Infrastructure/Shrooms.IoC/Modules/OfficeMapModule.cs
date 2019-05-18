using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.Domain.Services.OfficeMap;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
{
    class OfficeMapModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OfficeMapService>().As<IOfficeMapService>().InstancePerRequest();
        }
    }
}
