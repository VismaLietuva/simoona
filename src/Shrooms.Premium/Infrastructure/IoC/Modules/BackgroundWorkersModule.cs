using Autofac;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Premium.Main.PresentationLayer.API.BackgroundWorkers;

namespace Shrooms.Premium.Infrastructure.IoC.Modules
{
    public class BackgroundWorkersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NewEventNotifier>().As<IBackgroundWorker>().InstancePerDependency().AsSelf();
            builder.RegisterType<SharedEventNotifier>().As<IBackgroundWorker>().InstancePerDependency().AsSelf();
        }
    }
}
