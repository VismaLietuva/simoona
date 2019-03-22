using Autofac;
using Shrooms.Domain.Services.KudosShop;

namespace Shrooms.IoC.Modules
{
    public class KudosShopModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KudosShopService>().As<IKudosShopService>().InstancePerRequest();
        }
    }
}