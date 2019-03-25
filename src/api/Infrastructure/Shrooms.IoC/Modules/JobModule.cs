using Autofac;
using Shrooms.Domain.Services.Jobs;

namespace Shrooms.IoC.Modules
{
    public class JobModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JobService>().As<IJobService>().InstancePerRequest();
        }
    }
}
