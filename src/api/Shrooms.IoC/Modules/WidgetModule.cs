using Autofac;
using Shrooms.Domain.Services.Events;

namespace Shrooms.IoC.Modules
{
    public class WidgetModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EventWidgetService>().As<IEventWidgetService>().InstancePerRequest();
        }
    }
}
