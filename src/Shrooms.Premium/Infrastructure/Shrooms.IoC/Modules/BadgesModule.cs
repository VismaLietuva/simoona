using Autofac;
using Shrooms.Domain.Services.Badges;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
{
    public class BadgesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BadgesService>().As<IBadgesService>().InstancePerRequest();
        }
    }
}
