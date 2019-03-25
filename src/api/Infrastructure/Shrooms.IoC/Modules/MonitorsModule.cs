using Autofac;
using Shrooms.Domain.Services.Monitors;

namespace Shrooms.IoC.Modules
{
    public class MonitorsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MonitorService>().As<IMonitorService>().InstancePerRequest();
        }
    }
}
