using Autofac;
using Shrooms.Premium.Main.BusinessLayer.Domain.Services.KudosShop;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
{
    public class KudosShopModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<KudosShopService>().As<IKudosShopService>().InstancePerRequest();
        }
    }
}