using Autofac;
using Shrooms.Domain.Services.Jobs;
using Shrooms.Infrastructure.Interceptors;

namespace Shrooms.IoC.Modules
{
    public class JobModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JobService>().As<IJobService>().InstancePerRequest().EnableInterfaceTelemetryInterceptor();
        }
    }
}
