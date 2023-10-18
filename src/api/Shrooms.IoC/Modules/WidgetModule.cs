using Autofac;
using Shrooms.Domain.Services.Banners;
using Shrooms.Domain.Services.Events;

namespace Shrooms.IoC.Modules
{
    public class WidgetModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EventWidgetService>().As<IEventWidgetService>().InstancePerRequest();
            builder.RegisterType<BannerWidgetService>().As<IBannerWidgetService>().InstancePerRequest();
        }
    }
}
