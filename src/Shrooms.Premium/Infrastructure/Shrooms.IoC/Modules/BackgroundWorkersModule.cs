using Autofac;
using Shrooms.API.BackgroundWorkers;
using Shrooms.Infrastructure.FireAndForget;
using Shrooms.Premium.Main.PresentationLayer.Shrooms.API.BackgroundWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.Premium.Infrastructure.Shrooms.IoC.Modules
{
    public class BackgroundWorkersModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NewEventNotifier>().As<IBackgroundWorker>().InstancePerDependency().AsSelf();
            builder.RegisterType<SharedEventNotifier>().As<IBackgroundWorker>().InstancePerDependency().AsSelf();
            builder.RegisterType<ComiteeSuggestionNotifier>().As<IBackgroundWorker>().InstancePerDependency().AsSelf();
            builder.RegisterType<RemovedParticipantsNotifier>().As<IBackgroundWorker>().InstancePerDependency().AsSelf();
        }
    }
}
