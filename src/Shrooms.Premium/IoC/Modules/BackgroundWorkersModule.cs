using Autofac;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Premium.Presentation.Api.BackgroundWorkers;

namespace Shrooms.Premium.IoC.Modules
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
